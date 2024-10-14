using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Dto.Media.Art;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.StreetcodeArt.GetByStreetcodeId
{
  public class GetStreetcodeArtByStreetcodeIdHandler : IRequestHandler<GetStreetcodeArtByStreetcodeIdQuery, Result<IEnumerable<StreetcodeArtDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobAzureService _blobAzureService;

        public GetStreetcodeArtByStreetcodeIdHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            IBlobAzureService blobAzureService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _blobAzureService = blobAzureService;
        }

        public async Task<Result<IEnumerable<StreetcodeArtDto>>> Handle(GetStreetcodeArtByStreetcodeIdQuery request, CancellationToken cancellationToken)
        {
            var art = await _repositoryWrapper
            .StreetcodeArtRepository
            .GetAllAsync(
                predicate: s => s.StreetcodeId == request.StreetcodeId,
                include: art => art
                    .Include(a => a.Art)
                    .Include(i => i.Art.Image) !);

            if (art is null)
            {
                string errorMsg = $"Cannot find an art with corresponding streetcode id: {request.StreetcodeId}";
                throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
            }

            var artsDto = _mapper.Map<IEnumerable<StreetcodeArtDto>>(art);

            artsDto
                .Select(artDto => artDto.Art)
                .Where(art => art?.Image?.BlobName != null)
                .ToList()
                .ForEach(art => art.Image.Base64 = _blobAzureService.FindFileInStorageAsBase64(art.Image.BlobName));


            return Result.Ok(artsDto);
        }
    }
}