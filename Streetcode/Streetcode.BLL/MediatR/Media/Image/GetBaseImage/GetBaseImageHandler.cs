using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Image.GetBaseImage;

public class GetBaseImageHandler : IRequestHandler<GetBaseImageQuery, Result<MemoryStream>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public GetBaseImageHandler(IRepositoryWrapper repositoryWrapper, IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<MemoryStream>> Handle(GetBaseImageQuery request, CancellationToken cancellationToken)
    {
        var image = await _repositoryWrapper.ImageRepository.GetFirstOrDefaultAsync(a => a.Id == request.Id);

        if (image is null)
        {
            string errorMsg = $"Cannot find an image with corresponding id: {request.Id}";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        return _blobAzureService.FindFileInStorageAsMemoryStream(image.BlobName);
    }
}
