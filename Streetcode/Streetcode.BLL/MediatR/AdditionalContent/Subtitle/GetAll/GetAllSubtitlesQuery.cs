using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.AdditionalContent.Subtitles;

namespace Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetAll;

public record GetAllSubtitlesQuery : IRequest<Result<IEnumerable<SubtitleDto>>>;