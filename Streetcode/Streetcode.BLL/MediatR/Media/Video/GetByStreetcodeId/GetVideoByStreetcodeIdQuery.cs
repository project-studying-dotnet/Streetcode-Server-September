﻿using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Video;

namespace Streetcode.BLL.MediatR.Media.Video.GetByStreetcodeId;

public record GetVideoByStreetcodeIdQuery(int StreetcodeId): IRequest<Result<VideoDto>>;