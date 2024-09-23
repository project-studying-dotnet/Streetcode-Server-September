using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetById;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using NewsEntity = Streetcode.DAL.Entities.News.News;


namespace Streetcode.XUnitTest.MediatRTests.News
{
    public class GetNewsByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetNewsByIdHandler _handler;

        public GetNewsByIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _blobServiceMock = new Mock<IBlobService>();
            _handler = new GetNewsByIdHandler(_mapperMock.Object, _repositoryWrapperMock.Object, _blobServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenNewsFound()
        {
            // Arrange
            var newsId = 1;
            var request = new GetNewsByIdQuery(newsId);
            var news =  new NewsEntity() { Id = 1, Title = "Test News", Image = new Image { Id = 1 } };
            var newsDto =  new NewsDto() { Image = new ImageDto() };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetFirstOrDefaultAsync(It.Is<Expression<Func<NewsEntity, bool>>>(exp => exp.Compile().Invoke(news)),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(news);

            _mapperMock.Setup(m => m.Map<NewsDto>(news)).Returns(newsDto);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newsDto, result.Value);
            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<NewsEntity, bool>>>(
                    exp => exp.Compile().Invoke(new NewsEntity { Id = request.id })),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()), Times.Once);
            _mapperMock.Verify(m => m.Map<NewsDto>(news), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsErrorMsg_WhenNewsNotFound()
        {
            // Arrange
            var newsId = 1;
            var request = new GetNewsByIdQuery(newsId);
            string expectedErrorMsg = $"No news by entered Id - {request.id}";

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync((NewsEntity)null);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(expectedErrorMsg, result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(request, expectedErrorMsg), Times.Once);
        }
    }
}
