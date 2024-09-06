using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Audio;

namespace Streetcode.BLL.MediatR.Media.Audio.GetByStreetcodeId;

public record GetAudioByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<AudioDto>>;