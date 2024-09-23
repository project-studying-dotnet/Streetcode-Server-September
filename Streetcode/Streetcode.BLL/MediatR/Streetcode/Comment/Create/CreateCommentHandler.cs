using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

using commentEntety = Streetcode.DAL.Entities.Streetcode.TextContent.Comment;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Create
{
    public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, Result<CommentDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public CreateCommentHandler(IMapper mapper, IRepositoryWrapper repository, ILoggerService logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            var newComment = _mapper.Map<commentEntety>(request.Comment);

            if (newComment is null)
                throw new CustomException("Cannot convert null to comment", StatusCodes.Status204NoContent);

            var entity = await _repository.CommentRepository.CreateAsync(newComment);
            var resultIsSuccess = await _repository.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<CommentDto>(entity));
            }
            else
            {
                throw new CustomException("Failed to create a comment", StatusCodes.Status400BadRequest);
            }
        }
    }
}
