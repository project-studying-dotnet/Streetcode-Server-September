using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Image.GetAll;

public class GetAllImagesHandler : IRequestHandler<GetAllImagesQuery, Result<IEnumerable<ImageDto>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public GetAllImagesHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobAzureService blobAzureService)
    { 
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<IEnumerable<ImageDto>>> Handle(GetAllImagesQuery request, CancellationToken cancellationToken)
    {
        var images = await _repositoryWrapper.ImageRepository.GetAllAsync();

        if (images is null)
        {
            const string errorMsg = $"Cannot find any image";
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