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
using static StackExchange.Redis.Role;

namespace Streetcode.BLL.MediatR.Media.Audio.Create;

public class CreateAudioHandler : IRequestHandler<CreateAudioCommand, Result<AudioDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public CreateAudioHandler(
        IMapper mapper,
        IRepositoryWrapper repositoryWrapper,
        IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<AudioDto>> Handle(CreateAudioCommand request, CancellationToken cancellationToken)
    {
        _blobAzureService.SaveFileInStorage(
            request.Audio.BaseFormat,
            request.Audio.Title,
            string.Empty);

        var audio = _mapper.Map<DAL.Entities.Media.Audio>(request.Audio);

        audio.BlobName = request.Audio.Title;

        await _repositoryWrapper.AudioRepository.CreateAsync(audio);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        var createdAudio = _mapper.Map<AudioDto>(audio);

        if(resultIsSuccess)
        {
            return Result.Ok(createdAudio);
        }
        else
        {
            const string errorMsg = $"Failed to create an audio";
            throw new CustomException(errorMsg, StatusCodes.Status500InternalServerError);
        }
    }
}
