using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.MediatR.Streetcode.Comment.Create;

namespace Streetcode.WebApi.Controllers.Streetcode.TextContent;

public class CommentController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommentCreateDto commentCreateDto)
    {
        return HandleResult(await Mediator.Send(new CreateCommentCommand(commentCreateDto)));
    }
}
