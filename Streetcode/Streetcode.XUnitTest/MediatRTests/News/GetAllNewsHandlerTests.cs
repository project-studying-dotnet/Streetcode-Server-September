using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetAll;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatRTests.News
{
    public class GetAllNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetAllNewsHandler _handler;

        public GetAllNewsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _blobServiceMock = new Mock<IBlobService>();
            _handler = new GetAllNewsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsNewsDto_WhenNewsExist()
        {
            // Arrange
            var news = new List<NewsEntity> 
            { 
                new NewsEntity() { Id = 1, Title = "Test News", Image = new Image { Id = 1 } },
                new NewsEntity() { Id = 2, Title = "Test News" } 
            };
            var newsDtos = new List<NewsDto>() 
            { 
                new NewsDto() { Id = 1, Image = new ImageDto() },
                new NewsDto() { Id = 2 }
            };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(news);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<NewsDto>>(It.IsAny<IEnumerable<NewsEntity>>()))
                .Returns(newsDtos);

            // Act
            var result = await _handler.Handle(new GetAllNewsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newsDtos, result.Value);
        }

        [Fact]
        public async Task Handle_ReturnsErrorMsg_WhenNewsNotFound()
        {
            // Arrange
            string expectedErrorMsg = "There are no news in the database";

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
               .GetAllAsync(
                   It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                   It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))!
               .ReturnsAsync((IEnumerable<NewsEntity>)null);

            // Act
            var result = await _handler.Handle(new GetAllNewsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(expectedErrorMsg, result.Errors.First().Message);
            _loggerMock.Verify(x => x.LogError(It.IsAny<object>(), It.Is<string>(s => s.Contains(expectedErrorMsg))), Times.Once);
        }
    }
}
