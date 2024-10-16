﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetNewsAndLinksByUrl;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatRTests.News
{
    public class GetNewsAndLinksByUrlHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
        private readonly GetNewsAndLinksByUrlHandler _handler;

        public GetNewsAndLinksByUrlHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _blobAzureServiceMock = new Mock<IBlobAzureService>();
            _handler = new GetNewsAndLinksByUrlHandler(_mapperMock.Object, _repositoryWrapperMock.Object, _blobAzureServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsErrorMsg_WhenNewsNotFound()
        {
            // Arrange
            var url = "test1.com";
            var request = new GetNewsAndLinksByUrlQuery(url);
            string expectedErrorMsg = $"No news by entered Url - {url}";

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync((NewsEntity)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(async () =>
                await _handler.Handle(request, CancellationToken.None));

            Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
            Assert.Equal(expectedErrorMsg, exception.Message);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenNewsFound()
        {
            // Arrange
            var url = "test.com";
            var request = new GetNewsAndLinksByUrlQuery(url);
            var news1 = new NewsEntity { Id = 1, URL = "prev-url" };
            var news2 = new NewsEntity { Id = 2, URL = url };
            var news3 = new NewsEntity { Id = 3, URL = "next-url" };
            var newsDto = new NewsDto { Id = 2, URL = url };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetFirstOrDefaultAsync(It.Is<Expression<Func<NewsEntity, bool>>>(exp => exp.Compile().Invoke(news2)),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(news2);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(new List<NewsEntity> { news1, news2, news3 });

            _mapperMock.Setup(m => m.Map<NewsDto>(news2)).Returns(newsDto);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var newsWithURLsDto = result.Value;
            Assert.Equal("prev-url", newsWithURLsDto.PrevNewsUrl);
            Assert.Equal("next-url", newsWithURLsDto.NextNewsUrl);
            Assert.Equal(newsDto.Id, newsWithURLsDto.News.Id);
        }
    }
}
