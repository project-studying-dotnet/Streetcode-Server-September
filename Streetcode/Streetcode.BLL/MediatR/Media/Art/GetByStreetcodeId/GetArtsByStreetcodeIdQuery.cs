using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Art;

namespace Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId
{
  public record GetArtsByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<ArtDto>>>;
}
