using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;

namespace Streetcode.BLL.MediatR.Media.Image.Create;

public class CreateImageHandler : IRequestHandler<CreateImageCommand, Result<ImageDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public CreateImageHandler(
        IBlobAzureService blobAzureService,
        IRepositoryWrapper repositoryWrapper,
        IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<ImageDto>> Handle(CreateImageCommand request, CancellationToken cancellationToken)
    {
        _blobAzureService.SaveFileInStorage(
            request.Image.BaseFormat,
            request.Image.Title, string.Empty);

        var image = _mapper.Map<DAL.Entities.Media.Images.Image>(request.Image);

        image.BlobName = request.Image.Title;

        await _repositoryWrapper.ImageRepository.CreateAsync(image);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        var createdImage = _mapper.Map<ImageDto>(image);

        createdImage.Base64 = _blobAzureService.FindFileInStorageAsBase64(createdImage.BlobName);

        if (resultIsSuccess)
        {
            return Result.Ok(createdImage);
        }
        else
        {
            const string errorMsg = "Failed to create an image";
            throw new CustomException(errorMsg, StatusCodes.Status500InternalServerError);
        }
    }
}
