using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetRepliesByCommentId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;

public class GetRepliesByCommentIdHandler : IRequestHandler<GetRepliesByCommentIdQuery, Result<IEnumerable<CommentDto>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public GetRepliesByCommentIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CommentDto>>> Handle(GetRepliesByCommentIdQuery request, CancellationToken cancellationToken)
    {
        var commentsWithReplies = await _repositoryWrapper.CommentRepository
            .GetAllAsync(c => c.ParentCommentId == request.id);

        if (commentsWithReplies is null)
            throw new CustomException($"Cannot find any replies by the comment id: {request.id}", StatusCodes.Status204NoContent);

        var response = _mapper.Map<IEnumerable<CommentDto>>(commentsWithReplies);
        return Result.Ok(response);
    }
}