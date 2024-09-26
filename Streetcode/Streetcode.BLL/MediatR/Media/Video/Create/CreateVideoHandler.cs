using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Media.Video;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Video.Create;

using Video = DAL.Entities.Media.Video;

public class CreateVideoHandler: IRequestHandler<CreateVideoCommand, Result<VideoDto>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;

    public CreateVideoHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }
    
    public async Task<Result<VideoDto>> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
    {
        var video = _mapper.Map<Video>(request.VideoCreateDto) ?? throw new CustomException(
            "Can not map dto to entity", 
            StatusCodes.Status400BadRequest);

        var createdVideo = await _repositoryWrapper.VideoRepository.CreateAsync(video);
        bool isResultSuccess = await _repositoryWrapper.SaveChangesAsync() == 1;

        if (!isResultSuccess)
        {
            throw new CustomException("Can not create video", StatusCodes.Status500InternalServerError);
        }

        var createdVideoDto = _mapper.Map<VideoDto>(createdVideo) ?? throw new CustomException(
            "Can not map entity to dto", 
            StatusCodes.Status400BadRequest);

        return Result.Ok(createdVideoDto);
    }
}