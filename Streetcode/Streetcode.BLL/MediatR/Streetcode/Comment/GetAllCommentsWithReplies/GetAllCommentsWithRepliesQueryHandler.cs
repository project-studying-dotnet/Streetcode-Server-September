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
                var comments = await _repository.CommentRepository.GetAllAsync(
                    include: q => q.Include(c => c.Replies),
                    predicate: c => c.ParentCommentId == null);

                var commentDtos = _mapper.Map<List<CommentWithRepliesDto>>(comments);

                return Result.Ok(commentDtos);
            }
            catch (Exception ex)
            {
                return Result.Fail(new Error(ex.Message));
            }
        }
    }

}

