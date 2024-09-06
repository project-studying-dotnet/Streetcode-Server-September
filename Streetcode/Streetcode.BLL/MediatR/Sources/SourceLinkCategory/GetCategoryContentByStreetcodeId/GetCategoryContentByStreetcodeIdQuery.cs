using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Dto.Sources;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetCategoryContentByStreetcodeId
{
    public record GetCategoryContentByStreetcodeIdQuery(int streetcodeId, int categoryId) : IRequest<Result<StreetcodeCategoryContentDto>>;
}
