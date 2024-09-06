using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Sources;

namespace Streetcode.BLL.MediatR.Sources.SourceLink.GetCategoryById;

public record GetCategoryByIdQuery(int Id) : IRequest<Result<SourceLinkCategoryDto>>;