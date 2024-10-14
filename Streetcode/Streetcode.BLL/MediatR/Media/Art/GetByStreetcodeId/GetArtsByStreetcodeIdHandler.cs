using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Dto.Media.Art;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId
{
  public class GetArtsByStreetcodeIdHandler : IRequestHandler<GetArtsByStreetcodeIdQuery, Result<IEnumerable<ArtDto>>>
  {
        private readonly IBlobAzureService _blobAzureService;
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public GetArtsByStreetcodeIdHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            IBlobAzureService blobAzureService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _blobAzureService = blobAzureService;
        }

        public async Task<Result<IEnumerable<ArtDto>>> Handle(GetArtsByStreetcodeIdQuery request, CancellationToken cancellationToken)
        {
            var arts = await _repositoryWrapper.ArtRepository
                .GetAllAsync(
                predicate: sc => sc.StreetcodeArts.Any(s => s.StreetcodeId == request.StreetcodeId),
                include: scl => scl
                    .Include(sc => sc.Image) !);

            if (arts is null || !arts.Any())
            {
                string errorMsg = $"Cannot find any art with corresponding streetcode id: {request.StreetcodeId}";
                throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
            }

            var artsDto = _mapper.Map<IEnumerable<ArtDto>>(arts);
            artsDto
                .Where(artDto => artDto.Image != null && artDto.Image.BlobName != null)
                .ToList()
                .ForEach(artDto => artDto.Image!.Base64 = _blobAzureService.FindFileInStorageAsBase64(artDto.Image.BlobName!));

            return Result.Ok(artsDto);
        }
  }
}
