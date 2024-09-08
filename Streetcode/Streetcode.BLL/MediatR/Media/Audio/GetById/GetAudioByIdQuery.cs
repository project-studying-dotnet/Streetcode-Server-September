using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Audio;

namespace Streetcode.BLL.MediatR.Media.Audio.GetById;

public record GetAudioByIdQuery(int Id) : IRequest<Result<AudioDto>>;
