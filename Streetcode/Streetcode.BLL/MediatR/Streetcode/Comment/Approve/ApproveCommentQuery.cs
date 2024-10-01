using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Approve;

public record ApproveCommentQuery(int Id): IRequest<Result<Unit>>;