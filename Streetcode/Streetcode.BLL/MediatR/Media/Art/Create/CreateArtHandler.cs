using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Dto.Media.Art;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;
using StreetcodeArtEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeArt;

namespace Streetcode.BLL.MediatR.Media.Art.Create
{
    public class CreateArtHandler : IRequestHandler<CreateArtCommand, Result<ArtCreateDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public CreateArtHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<ArtCreateDto>> Handle(CreateArtCommand request, CancellationToken cancellationToken)
        {
            var arts = await _repositoryWrapper.ArtRepository.GetAllAsync();
            if (arts.Any(a => a.ImageId == request.newArt.ImageId))
            {
                string errorMsg = $"An art with Image Id: {request.newArt.ImageId} already exists.\n" +
                                   "Please choose a different image.";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var newArt = _mapper.Map<ArtEntity>(request.newArt);
            if (newArt == null)
            {
                const string errorMsg = "Cannot convert null to an art.";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            newArt.StreetcodeArts.Clear();
            newArt = await _repositoryWrapper.ArtRepository.CreateAsync(newArt);
            await _repositoryWrapper.SaveChangesAsync();

            var streetcodeIds = request.newArt.Streetcodes!.Select(s => s.Id).ToList();

            var streetcodes = await _repositoryWrapper.StreetcodeRepository
                .GetAllAsync(s => streetcodeIds.Contains(s.Id));

            var streetcodeArts = streetcodes.
                Select(sta => new StreetcodeArtEntity
                {
                    ArtId = newArt.Id,
                    Art = newArt,
                    StreetcodeId = sta.Id,
                    Streetcode = sta
                }).ToList();

            newArt.StreetcodeArts.AddRange(streetcodeArts);
            bool resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<ArtCreateDto>(newArt));
            }
            else 
            { const string errorMsg = "Failed to save an art";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
            }  
        }
    }
}
