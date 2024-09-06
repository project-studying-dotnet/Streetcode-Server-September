using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Audio;

namespace Streetcode.BLL.MediatR.Media.Audio.Create;

public record CreateAudioCommand(AudioFileBaseCreateDto Audio) : IRequest<Result<AudioDto>>;
