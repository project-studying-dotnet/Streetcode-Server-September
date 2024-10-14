using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetRepliesByCommentId;

public record GetRepliesByCommentIdQuery(int id) : IRequest<Result<IEnumerable<CommentDto>>>;