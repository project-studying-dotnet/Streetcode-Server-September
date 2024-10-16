using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.MediatR.Media.Audio.GetAll;

public class GetAllAudiosHandler : IRequestHandler<GetAllAudiosQuery, Result<IEnumerable<AudioDto>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public GetAllAudiosHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<IEnumerable<AudioDto>>> Handle(GetAllAudiosQuery request, CancellationToken cancellationToken)
    {
        var audios = await _repositoryWrapper.AudioRepository.GetAllAsync();

        if (audios is null)
        {
            const string errorMsg = "Cannot find any audios";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        var audioDtos = _mapper.Map<IEnumerable<AudioDto>>(audios);
        foreach (var audio in audioDtos)
        {
            audio.Base64 = _blobAzureService.FindFileInStorageAsBase64(audio.BlobName);
        }

        return Result.Ok(audioDtos);
    }
}