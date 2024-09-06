using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.RelatedFigure;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetAllCatalog
{
  public record GetAllStreetcodesCatalogQuery(int page, int count) : IRequest<Result<IEnumerable<RelatedFigureDto>>>;
}
