using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;
using StreetcodeArtEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeArt;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Dto.Media.Art;

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
            var newArt = _mapper.Map<ArtEntity>(request.newArt);
            if (newArt is null)
            {
                const string errorMsg = "Cannot convert null to art";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }
            

            if (newArt.ImageId == 0)
            {
                newArt.Image = null;
            }

            var entity = await _repositoryWrapper.ArtRepository.CreateAsync(newArt);
            await _repositoryWrapper.SaveChangesAsync();

            var streetcodeExists = await _repositoryWrapper.StreetcodeRepository.
                GetFirstOrDefaultAsync(s => s.Id == request.newArt.StreetcodeId);


            if (streetcodeExists != null) 
            {
                var streetcodeArt = new StreetcodeArtEntity()
                {
                    StreetcodeId = streetcodeExists.Id,
                    ArtId = entity.Id
                };
                await _repositoryWrapper.StreetcodeArtRepository.CreateAsync(streetcodeArt);
                await _repositoryWrapper.SaveChangesAsync();
            }

            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
            Console.WriteLine($"_______________{resultIsSuccess}_____________");
            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<ArtCreateDto>(entity));
            }
            else
            {
                const string errorMsg = "Failed to create an art";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
