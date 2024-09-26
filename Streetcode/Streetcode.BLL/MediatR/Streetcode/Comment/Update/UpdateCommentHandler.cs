using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

using CommentEntety = Streetcode.DAL.Entities.Streetcode.TextContent.Comment;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Update
{
    public class UpdateCommentHandler : IRequestHandler<UpdateCommentCommand, Result<CommentDto>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public UpdateCommentHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }

        public async Task<Result<CommentDto>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = _mapper.Map<CommentEntety>(request.Comment);

            if (comment is null)
                throw new CustomException($"Cannot convert null to Comment", StatusCodes.Status204NoContent);

            var commentEntety = await _repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(
                x => x.Id == comment.Id,
                null);

            if (commentEntety is null)
                throw new CustomException($"Comment with id {comment.Id} not found", StatusCodes.Status404NotFound);

            comment.DateCreated = commentEntety.DateCreated;
            comment.StreetcodeId = commentEntety.StreetcodeId;
            comment.UserId = commentEntety.UserId;

            var response = comment;

            _repositoryWrapper.CommentRepository.Update(comment);
            bool resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<CommentDto>(response));
            }
            else
            {
                throw new CustomException($"Failed to update Comment", StatusCodes.Status400BadRequest);
            }
        }
    }
}
