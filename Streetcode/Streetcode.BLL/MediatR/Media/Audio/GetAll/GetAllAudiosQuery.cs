using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Audio;

namespace Streetcode.BLL.MediatR.Media.Audio.GetAll;

public record GetAllAudiosQuery : IRequest<Result<IEnumerable<AudioDto>>>;