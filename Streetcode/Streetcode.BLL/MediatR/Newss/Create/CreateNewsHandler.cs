using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Newss.Create
{
    public class CreateNewsHandler : IRequestHandler<CreateNewsCommand, Result<NewsDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        public CreateNewsHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<NewsDto>> Handle(CreateNewsCommand request, CancellationToken cancellationToken)
        {
            var newNews = _mapper.Map<News>(request.newNews);
            if (newNews is null)
            {
                const string errorMsg = "Cannot convert null to news";
                throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
            }

            if (newNews.ImageId == 0)
            {
                newNews.ImageId = null;
            }

            var entity = await _repositoryWrapper.NewsRepository.CreateAsync(newNews);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
            if(resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<NewsDto>(entity));
            }
            else
            {
                const string errorMsg = "Failed to create a news";
                throw new CustomException(errorMsg, StatusCodes.Status500InternalServerError);
            }
        }
    }
}
