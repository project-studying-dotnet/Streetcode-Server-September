using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Audio.Delete;

public class DeleteAudioHandler : IRequestHandler<DeleteAudioCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;
    public DeleteAudioHandler(IRepositoryWrapper repositoryWrapper, IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<Unit>> Handle(DeleteAudioCommand request, CancellationToken cancellationToken)
    {
        var audio = await _repositoryWrapper.AudioRepository.GetFirstOrDefaultAsync(a => a.Id == request.Id);

        if (audio is null)
        {
            string errorMsg = $"Cannot find an audio with corresponding categoryId: {request.Id}";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        _repositoryWrapper.AudioRepository.Delete(audio);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            _blobAzureService.DeleteFileInStorage(audio.BlobName);
        }

        if (resultIsSuccess)
        {
            return Result.Ok(Unit.Value);
        }
        else
        {
            string errorMsg = $"Failed to delete an audio";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }
    }
}
