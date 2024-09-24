using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Delete
{
    public record DeleteCommentCommand(int id) : IRequest<Result<Unit>>;
}
