using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Dto.Media.Art;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;
using StreetcodeArtEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeArt;

namespace Streetcode.BLL.MediatR.Media.Art.Create
{
    public class CreateArtHandler : IRequestHandler<CreateArtCommand, Result<ArtDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public CreateArtHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<ArtDto>> Handle(CreateArtCommand request, CancellationToken cancellationToken)
        {
            var arts = await _repositoryWrapper.ArtRepository.GetAllAsync();
            if (arts.Any(a => a.ImageId == request.newArt.ImageId))
            {
                string errorMsg = $"An art with Image Id: {request.newArt.ImageId} already exists. " +
                                   "Please choose a different image.";
                throw new CustomException(errorMsg, StatusCodes.Status400BadRequest);
            }

            var newArt = _mapper.Map<ArtEntity>(request.newArt);
            if (newArt == null)
            {
                throw new CustomException("Cannot convert null to an art.", StatusCodes.Status400BadRequest);
            }

            newArt.StreetcodeArts.Clear();
            newArt = await _repositoryWrapper.ArtRepository.CreateAsync(newArt);

            var streetcodeIds = request.newArt.StreetcodeIds!.ToList();

            var streetcodes = await _repositoryWrapper.StreetcodeRepository
                .GetAllAsync(s => streetcodeIds.Contains(s.Id));

            var streetcodeArts = streetcodes.
                Select(sta => new StreetcodeArtEntity
                {
                    ArtId = newArt.Id,
                    StreetcodeId = sta.Id
                }).ToList();

            newArt.StreetcodeArts.AddRange(streetcodeArts);
            bool resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<ArtDto>(newArt));
            }
            else
            {
                throw new CustomException("Failed to save an art", StatusCodes.Status400BadRequest);
            }
        }
    }
}
