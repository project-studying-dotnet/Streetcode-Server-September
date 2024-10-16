using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Delete;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using NewsEntity = Streetcode.DAL.Entities.News.News;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;

namespace Streetcode.XUnitTest.MediatRTests.News
{
    public class DeleteNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly DeleteNewsHandler _handler;

        public DeleteNewsHandlerTests() 
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new DeleteNewsHandler(_repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnSuccess_WhenNewsFound() 
        {
            var newsId = 1;
            var command = new DeleteNewsCommand(newsId);
            var news = new NewsEntity() { Id = 1, Title = "Test News" };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetFirstOrDefaultAsync(It.Is<Expression<Func<NewsEntity, bool>>>(exp => exp.Compile().Invoke(news)),
                null)).ReturnsAsync(news);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.Delete(news));
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.Delete(news), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnSuccess_WhenNewsHasImage()
        {
            var newsId = 1;
            var command = new DeleteNewsCommand(newsId);
            var image = new ImageEntity() { Id = 1 };
            var news = new NewsEntity() { Id = 1, Title = "Test News", Image = image };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetFirstOrDefaultAsync(It.Is<Expression<Func<NewsEntity, bool>>>(exp => exp.Compile().Invoke(news)),
                It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>())).ReturnsAsync(news);

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.Delete(news.Image));
            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.Delete(news));
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.Delete(news), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.ImageRepository.Delete(It.Is<Image>(img => img.Id == 1)), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ThrowsCustomException_WhenNewsNotFound()
        {
            // Arrange
            var newsId = 1;
            var command = new DeleteNewsCommand(newsId);
            var expectedErrorMsg = $"No news found by entered Id - {newsId}";

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<NewsEntity, bool>>>(), null))
                .ReturnsAsync((NewsEntity)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal(expectedErrorMsg, exception.Message);
            Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.Delete(It.IsAny<NewsEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ThrowsCustomException_WhenSaveChangesFailed()
        {
            // Arrange
            var newsId = 1;
            var command = new DeleteNewsCommand(newsId);
            var expectedErrorMsg = "Failed to delete news";
            var news = new NewsEntity { Id = 1, Title = "Test News" };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<NewsEntity, bool>>>(), null))
                .ReturnsAsync(news);

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal(expectedErrorMsg, exception.Message);
            Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.Delete(news), Times.Once);
        }
    }
}
