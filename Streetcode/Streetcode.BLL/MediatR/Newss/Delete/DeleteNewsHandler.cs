using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Newss.Delete
{
    public class DeleteNewsHandler : IRequestHandler<DeleteNewsCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public DeleteNewsHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<Unit>> Handle(DeleteNewsCommand request, CancellationToken cancellationToken)
        {
            int id = request.id;
            var news = await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(n => n.Id == id);
            if (news == null)
            {
                string errorMsg = $"No news found by entered Id - {id}";
                throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
            }

            if (news.Image is not null)
            {
                _repositoryWrapper.ImageRepository.Delete(news.Image);
            }

            _repositoryWrapper.NewsRepository.Delete(news);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
            if(resultIsSuccess)
            {
                return Result.Ok(Unit.Value);
            }
            else
            {
                string errorMsg = "Failed to delete news";
                throw new CustomException(errorMsg, StatusCodes.Status500InternalServerError);
            }
        }
    }
}
