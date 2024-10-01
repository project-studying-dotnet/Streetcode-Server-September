using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Extensions;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Approve;

public class ApproveCommentHandler: IRequestHandler<ApproveCommentQuery, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IStringLocalizer<ErrorMessages> _stringLocalizer;

    public ApproveCommentHandler(IRepositoryWrapper repositoryWrapper, IStringLocalizer<ErrorMessages> stringLocalizer)
    {
        _repositoryWrapper = repositoryWrapper;
        _stringLocalizer = stringLocalizer;
    }
    
    public async Task<Result<Unit>> Handle(ApproveCommentQuery request, CancellationToken cancellationToken)
    {
        var comment = await _repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(c => c.Id == request.Id);

        if (comment is null)
        {
            string errorMessage = _stringLocalizer.GetErrorMessage(ErrorKeys.NotFoundError, nameof(Comment), request.Id);
            throw new CustomException(errorMessage, StatusCodes.Status404NotFound);
        }

        comment.Status = comment.Status == CommentStatus.Pending
            ? CommentStatus.Approved
            : throw new CustomException("Comment can not be approved twice!", StatusCodes.Status400BadRequest);

        _repositoryWrapper.CommentRepository.Update(comment);
        bool isSaveSuccessful = await _repositoryWrapper.SaveChangesAsync() == 1;

        if (!isSaveSuccessful)
        {
            string errorMessage = _stringLocalizer.GetErrorMessage(ErrorKeys.SaveChangesError);
            throw new CustomException(errorMessage, StatusCodes.Status500InternalServerError);
        }

        return Result.Ok(Unit.Value);
    }
}