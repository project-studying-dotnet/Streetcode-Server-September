using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Dto.Sources;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll
{
    public record GetAllCategoryNamesQuery : IRequest<Result<IEnumerable<CategoryWithNameDto>>>;
}
