using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.AdditionalContent.Subtitles;

namespace Streetcode.BLL.MediatR.AdditionalContent.GetById;

public record GetSubtitleByIdQuery(int Id) : IRequest<Result<SubtitleDto>>;
