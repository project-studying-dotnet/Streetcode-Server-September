using MediatR;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;

using FluentResults;
using Dto.Sources;

public record CreateSourceLinkCategoryCommand(SourceLinkCategoryCreateDto SrcLinkCategoryCreateDto)
    : IRequest<Result<Unit>>;