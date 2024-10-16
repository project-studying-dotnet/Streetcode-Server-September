using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Image.Delete;

public class DeleteImageHandler : IRequestHandler<DeleteImageCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public DeleteImageHandler(IRepositoryWrapper repositoryWrapper, IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<Unit>> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _repositoryWrapper.ImageRepository
            .GetFirstOrDefaultAsync(
            predicate: i => i.Id == request.Id,
            include: s => s.Include(i => i.Streetcodes));

        if (image is null)
        {
            string errorMsg = $"Cannot find an image with corresponding categoryId: {request.Id}";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        _repositoryWrapper.ImageRepository.Delete(image);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            _blobAzureService.DeleteFileInStorage(image.BlobName);
        }

        if(resultIsSuccess)
        {
            return Result.Ok(Unit.Value);
        }
        else
        {
            const string errorMsg = $"Failed to delete an image";
            throw new CustomException(errorMsg, StatusCodes.Status500InternalServerError);
        }
    }
}