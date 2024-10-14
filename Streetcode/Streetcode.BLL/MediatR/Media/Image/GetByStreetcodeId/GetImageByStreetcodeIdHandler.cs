using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;

namespace Streetcode.BLL.MediatR.Media.Image.GetByStreetcodeId;

public class GetImageByStreetcodeIdHandler : IRequestHandler<GetImageByStreetcodeIdQuery, Result<IEnumerable<ImageDto>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public GetImageByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper,
        IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<IEnumerable<ImageDto>>> Handle(GetImageByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var images = (await _repositoryWrapper.ImageRepository
            .GetAllAsync(
            f => f.Streetcodes.Any(s => s.Id == request.StreetcodeId),
              include: q => q.Include(img => img.ImageDetails)))
                                         .OrderBy(img => img.ImageDetails?.Title);

        if (!images.Any())
        {
            string errorMsg = $"Cannot find an image with the corresponding streetcode id: {request.StreetcodeId}";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        var imageDtos = _mapper.Map<IEnumerable<ImageDto>>(images);

        foreach (var image in imageDtos)
        {
            image.Base64 = _blobAzureService.FindFileInStorageAsBase64(image.BlobName);
        }

        return Result.Ok(imageDtos);
    }
}