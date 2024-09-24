using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Video;

namespace Streetcode.BLL.MediatR.Media.Video.Create;

public record CreateVideoCommand(VideoCreateDto VideoCreateDto): IRequest<Result<VideoDto>>;