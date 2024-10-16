using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.MediatR.Streetcode.Comment.Approve;
using Streetcode.BLL.MediatR.Streetcode.Comment.Create;
using Streetcode.BLL.MediatR.Streetcode.Comment.Delete;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetAllCommentsWithReplies;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetRepliesByCommentId;
using Streetcode.BLL.MediatR.Streetcode.Comment.Update;
using Streetcode.WebApi.Extensions.Attributes;

namespace Streetcode.WebApi.Controllers.Streetcode.TextContent;

public class CommentController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommentCreateDto commentCreateDto)
    {
        return HandleResult(await Mediator.Send(new CreateCommentCommand(commentCreateDto)));
    }

    [HttpDelete("{id:int}")]
    [AuthorizeRoleOrOwner("Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteCommentCommand(id)));
    }

    [HttpPut]
    [AuthorizeRoleOrOwner("None")]
    public async Task<IActionResult> Update([FromBody] CommentUpdateDto commentDto)
    {
        return HandleResult(await Mediator.Send(new UpdateCommentCommand(commentDto)));
    }

    [HttpGet]
    public async Task<IActionResult> GetCommentsWithReplies()
    {
       return HandleResult(await Mediator.Send(new GetAllCommentsWithRepliesQuery()));      
    }

    [HttpGet("{streetcodeId:int}")]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        return HandleResult(await Mediator.Send(new GetCommentByStreetcodeIdQuery(streetcodeId)));
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new ApproveCommentQuery(id)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRepliesByCommentId([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetRepliesByCommentIdQuery(id)));
    }
}
