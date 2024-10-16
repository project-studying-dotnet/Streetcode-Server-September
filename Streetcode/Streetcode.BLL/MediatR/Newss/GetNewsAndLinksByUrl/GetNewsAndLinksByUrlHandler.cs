using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.BLL.MediatR.Newss.GetNewsAndLinksByUrl
{
    public class GetNewsAndLinksByUrlHandler : IRequestHandler<GetNewsAndLinksByUrlQuery, Result<NewsWithURLsDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobAzureService _blobAzureService;
        public GetNewsAndLinksByUrlHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, IBlobAzureService blobAzureService)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _blobAzureService = blobAzureService;
        }

        public async Task<Result<NewsWithURLsDto>> Handle(GetNewsAndLinksByUrlQuery request, CancellationToken cancellationToken)
        {
            string url = request.url;
            var newsDto = _mapper.Map<NewsDto>(await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
                predicate: sc => sc.URL == url,
                include: scl => scl
                    .Include(sc => sc.Image)));

            if (newsDto is null)
            {
                string errorMsg = $"No news by entered Url - {url}";
                throw new CustomException(errorMsg, StatusCodes.Status404NotFound);
            }

            if (newsDto.Image is not null)
            {
                newsDto.Image.Base64 = _blobAzureService.FindFileInStorageAsBase64(newsDto.Image.BlobName);
            }

            var news = (await _repositoryWrapper.NewsRepository.GetAllAsync()).ToList();
            var newsIndex = news.FindIndex(x => x.Id == newsDto.Id);
            string? prevNewsLink = null;
            string? nextNewsLink = null;

            if(newsIndex != 0)
            {
                prevNewsLink = news[newsIndex - 1].URL;
            }

            if(newsIndex != news.Count - 1)
            {
                nextNewsLink = news[newsIndex + 1].URL;
            }

            var randomNewsTitleAndLink = new RandomNewsDto();

            var arrCount = news.Count;
            if (arrCount > 3)
            {
                if (newsIndex + 1 == arrCount - 1 || newsIndex == arrCount - 1)
                {
                    randomNewsTitleAndLink.RandomNewsUrl = news[newsIndex - 2].URL;
                    randomNewsTitleAndLink.Title = news[newsIndex - 2].Title;
                }
                else
                {
                    randomNewsTitleAndLink.RandomNewsUrl = news[arrCount - 1].URL;
                    randomNewsTitleAndLink.Title = news[arrCount - 1].Title;
                }
            }
            else
            {
                randomNewsTitleAndLink.RandomNewsUrl = news[newsIndex].URL;
                randomNewsTitleAndLink.Title = news[newsIndex].Title;
            }

            var newsDtoWithUrls = new NewsWithURLsDto
            {
                RandomNews = randomNewsTitleAndLink,
                News = newsDto,
                NextNewsUrl = nextNewsLink,
                PrevNewsUrl = prevNewsLink
            };

            return Result.Ok(newsDtoWithUrls);
        }
    }
}