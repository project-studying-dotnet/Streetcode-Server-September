using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.AdditionalContent;

namespace Streetcode.BLL.MediatR.AdditionalContent.Tag.GetAll;

public record GetAllTagsQuery : IRequest<Result<IEnumerable<TagDto>>>;
