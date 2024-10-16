using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Audio.GetBaseAudio;

public class GetBaseAudioHandler : IRequestHandler<GetBaseAudioQuery, Result<MemoryStream>>
{

    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public GetBaseAudioHandler(IBlobAzureService blobAzureService, IRepositoryWrapper repositoryWrapper)
    {
        _blobAzureService = blobAzureService;
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<Result<MemoryStream>> Handle(GetBaseAudioQuery request, CancellationToken cancellationToken)
    {
        var audio = await _repositoryWrapper.AudioRepository.GetFirstOrDefaultAsync(a => a.Id == request.Id);

        if (audio is null)
        {
            string errorMsg = $"Cannot find an audio with corresponding id: {request.Id}";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        return _blobAzureService.FindFileInStorageAsMemoryStream(audio.BlobName);
    }
}
