using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.ResultVariations;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.MediatR.Media.Audio.GetByStreetcodeId;

public class GetAudioByStreetcodeIdQueryHandler : IRequestHandler<GetAudioByStreetcodeIdQuery, Result<AudioDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobAzureService _blobAzureService;

    public GetAudioByStreetcodeIdQueryHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobAzureService blobAzureService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobAzureService = blobAzureService;
    }

    public async Task<Result<AudioDto>> Handle(GetAudioByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var streetcode = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(
            s => s.Id == request.StreetcodeId,
            include: q => q.Include(s => s.Audio) !);
        if (streetcode == null)
        {
            string errorMsg = $"Cannot find an audio with the corresponding streetcode id: {request.StreetcodeId}";
            throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
        }

        NullResult<AudioDto> result = new NullResult<AudioDto>();

        if (streetcode.Audio != null)
        {
            AudioDto audioDto = _mapper.Map<AudioDto>(streetcode.Audio);
            audioDto.Base64 = _blobAzureService.FindFileInStorageAsBase64(audioDto.BlobName);
            result.WithValue(audioDto);
        }

        return result;
    }
}