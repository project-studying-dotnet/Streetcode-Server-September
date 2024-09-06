using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Media.Art;

namespace Streetcode.BLL.MediatR.Media.StreetcodeArt.GetByStreetcodeId
{
  public record GetStreetcodeArtByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<StreetcodeArtDto>>>;
}
