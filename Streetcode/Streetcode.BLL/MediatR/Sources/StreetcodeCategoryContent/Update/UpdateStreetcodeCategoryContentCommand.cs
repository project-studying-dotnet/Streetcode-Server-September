using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Sources;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update
{
    public record UpdateStreetcodeCategoryContentCommand(int categoryId, 
                                                        SourceLinkCategoryContentUpdateDto CategoryContentUpdateDto)
    : IRequest<Result<SourceLinkCategoryContentUpdateDto>>;
}
