using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.GetByStreetcodeId;

public class GetCommentByStreetcodeIdHandler : IRequestHandler<GetCommentByStreetcodeIdQuery, Result<IEnumerable<CommentDto>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public GetCommentByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CommentDto>>> Handle(GetCommentByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await _repositoryWrapper.CommentRepository
            .GetAllAsync(c => c.StreetcodeId == request.StreetcodeId);

        if (comment is null)
            throw new CustomException($"Cannot find any comment by the streetcode id: {request.StreetcodeId}", StatusCodes.Status204NoContent);

        var response = _mapper.Map<IEnumerable<CommentDto>>(comment);
        return Result.Ok(response);
    }
}