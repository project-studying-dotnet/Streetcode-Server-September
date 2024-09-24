using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Art;

namespace Streetcode.BLL.MediatR.Media.Art.Create;

public record CreateArtCommand(ArtCreateDto newArt) : IRequest<Result<ArtDto>>;
