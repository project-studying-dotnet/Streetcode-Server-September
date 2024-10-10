using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.SortedByDateTime;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatRTests.News
{
    public class SortedByDateTimeHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly SortedByDateTimeHandler _handler;

        public SortedByDateTimeHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _blobServiceMock = new Mock<IBlobService>();
            _handler = new SortedByDateTimeHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSortedNews_WhenNewsExists()
        {
            // Arrange
            var request = new SortedByDateTimeQuery();
            var news = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, Title = "News A", CreationDate = new DateTime(2024, 9, 19) },
                new NewsEntity { Id = 2, Title = "News B", CreationDate = new DateTime(2024, 9, 20) }
            };

            var newsDtos = new List<NewsDto>
            {
                new NewsDto { Id = 1, Title = "News A", CreationDate = new DateTime(2024, 9, 19) },
                new NewsDto { Id = 2, Title = "News B", CreationDate = new DateTime(2024, 9, 20) }
            };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(news);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<NewsDto>>(news))
                .Returns(newsDtos);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var sortedNews = result.Value;
            Assert.Equal(2, sortedNews.Count);
            Assert.Equal(2, sortedNews[0].Id);
            Assert.Equal(1, sortedNews[1].Id);
            Assert.Equal(newsDtos.OrderByDescending(x => x.CreationDate).ToList(), sortedNews);
        }

        [Fact]
        public async Task Handle_ReturnsNewsDtosWithBase64_WhenImagesExist()
        {
            // Arrange
            var request = new SortedByDateTimeQuery();
            var news = new List<NewsEntity>
            {
                new NewsEntity
                {
                    Id = 1,
                    Title = "News A",
                    CreationDate = new DateTime(2024, 9, 19),
                    Image = new Image { BlobName = "image1" }
                }
            };

            var newsDtos = new List<NewsDto>
            {
                new NewsDto
                {
                    Id = 1,
                    Title = "News A",
                    CreationDate = new DateTime(2024, 9, 19),
                    Image = new ImageDto { BlobName = "image1" }
                }
            };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(news);


            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<NewsDto>>(news))
                .Returns(newsDtos);

            _blobServiceMock.Setup(blob => blob.FindFileInStorageAsBase64("image1"))
                .Returns("base64image");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            var resultNewsDtos = result.Value;
            Assert.Single(resultNewsDtos);
            Assert.Equal("base64image", resultNewsDtos.First().Image.Base64);
        }


        [Fact]
        public async Task Handle_ReturnsFail_WhenNoNewsFound()
        {
            // Arrange
            var request = new SortedByDateTimeQuery();
            const string expectedErrorMsg = "There are no news in the database";

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))!
                .ReturnsAsync((IEnumerable<NewsEntity>)null);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedErrorMsg, result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(request, expectedErrorMsg), Times.Once);
        }
    }
}
