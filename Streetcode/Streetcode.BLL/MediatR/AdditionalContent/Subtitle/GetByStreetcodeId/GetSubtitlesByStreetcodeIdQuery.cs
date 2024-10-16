﻿using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.AdditionalContent.Subtitles;

namespace Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetByStreetcodeId
{
    public record GetSubtitlesByStreetcodeIdQuery(int StreetcodeId): IRequest<Result<SubtitleDto>>;
}
