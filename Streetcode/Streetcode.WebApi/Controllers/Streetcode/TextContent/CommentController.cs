﻿using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.MediatR.Streetcode.Comment.Approve;
using Streetcode.BLL.MediatR.Streetcode.Comment.Create;
using Streetcode.BLL.MediatR.Streetcode.Comment.Delete;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetAllCommentsWithReplies;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Streetcode.Comment.Update;

namespace Streetcode.WebApi.Controllers.Streetcode.TextContent;

public class CommentController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommentCreateDto commentCreateDto)
    {
        return HandleResult(await Mediator.Send(new CreateCommentCommand(commentCreateDto)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteCommentCommand(id)));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CommentUpdateDto commentDto)
    {
        return HandleResult(await Mediator.Send(new UpdateCommentCommand(commentDto)));
    }

    [HttpGet]
    //[Authorize(Roles = "Admin")]
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
    public async Task<IActionResult> Approve([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new ApproveCommentQuery(id)));
    }
}
