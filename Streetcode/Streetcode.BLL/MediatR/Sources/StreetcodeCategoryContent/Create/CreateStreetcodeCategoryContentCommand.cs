using MediatR;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;

using FluentResults;
using Dto.Sources;

public record CreateStreetcodeCategoryContentCommand(CategoryContentCreateDto CategoryContentCreateDto)
    : IRequest<Result<Unit>>;