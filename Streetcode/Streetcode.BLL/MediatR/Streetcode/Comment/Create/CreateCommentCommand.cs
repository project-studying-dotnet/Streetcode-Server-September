using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Create
{
    public record CreateCommentCommand(CommentCreateDto Comment) : IRequest<Result<CommentDto>>;
}
