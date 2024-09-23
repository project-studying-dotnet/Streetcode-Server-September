using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Delete
{
    public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;
        public DeleteCommentHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            int id = request.id;
            var comment = await _repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                throw new CustomException($"No comment found by entered Id - {id}", StatusCodes.Status204NoContent);

            _repositoryWrapper.CommentRepository.Delete(comment);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(Unit.Value);
            }
            else
            {
                throw new CustomException("Failed to delete comment", StatusCodes.Status400BadRequest);
            }
        }
    }
}