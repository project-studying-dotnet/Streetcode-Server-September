using AutoMapper;
using Azure.Storage.Blobs;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;

namespace Streetcode.BLL.MediatR.Media.Audio.GetById;

public class GetAudioByIdHandler : IRequestHandler<GetAudioByIdQuery, Result<AudioDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public GetAudioByIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<AudioDto>> Handle(GetAudioByIdQuery request, CancellationToken cancellationToken)
    {
        var audio = await _repositoryWrapper.AudioRepository.GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (audio is null)
        {
            string errorMsg = $"Cannot find an audio with corresponding id: {request.Id}";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        var audioDto = _mapper.Map<AudioDto>(audio);

        audioDto.Base64 = _blobAzureService.FindFileInStorageAsBase64(audioDto.BlobName);

        return Result.Ok(audioDto);
    }
}