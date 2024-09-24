using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Update
{
    public record UpdateCommentCommand(CommentUpdateDto Comment) : IRequest<Result<CommentDto>>;
}