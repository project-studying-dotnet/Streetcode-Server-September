using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.GetAllCommentsWithReplies
{
    public class GetAllCommentsWithRepliesQueryHandler: IRequestHandler<GetAllCommentsWithRepliesQuery, Result<List<CommentWithRepliesDto>>>
    {
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;

        public GetAllCommentsWithRepliesQueryHandler(IRepositoryWrapper repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<List<CommentWithRepliesDto>>> Handle(GetAllCommentsWithRepliesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var comments = await _repository.CommentRepository.GetAllAsync();
                var commentDtos = _mapper.Map<List<CommentWithRepliesDto>>(comments);
                var commentDict = commentDtos.ToDictionary(c => c.Id);

                foreach (var commentDto in commentDtos)
                {
                    commentDto.Replies = new List<CommentWithRepliesDto>();
                }

                foreach (var commentDto in commentDtos)
                {
                    if (commentDto.ParentCommentId != null && commentDict.TryGetValue(commentDto.ParentCommentId.Value, out var parentComment))
                    {
                        parentComment.Replies.Add(commentDto);
                    }
                }

                var topLevelComments = commentDtos.Where(c => c.ParentCommentId == null).ToList();

                return Result.Ok(topLevelComments);
            }
            catch (Exception ex)
            {
                return Result.Fail(new Error(ex.Message));
            }
        }
    }
}

