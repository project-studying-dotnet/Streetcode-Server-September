using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;

public record GetCommentByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<CommentDto>>>;