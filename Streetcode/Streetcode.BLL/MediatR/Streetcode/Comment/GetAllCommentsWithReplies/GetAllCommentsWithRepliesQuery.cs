using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetAllCommentsWithReplies
{
    public record GetAllCommentsWithRepliesQuery() : IRequest<Result<List<CommentWithRepliesDto>>> { }
}
