using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatRTests.News
{
    public class CreateNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly CreateNewsHandler _handler;

        public CreateNewsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new CreateNewsHandler(_mapperMock.Object, _repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenNewsIsCreatedSuccessfully() 
        {
            // Arrange 
            var testNews = new NewsEntity { Id = 1, Title = "Test News" };
            var testNewsDto = new NewsDto { Id = 1, Title = "Test News" };
            var command = new CreateNewsCommand(testNewsDto);

            _mapperMock.Setup(m => m.Map<NewsEntity>(command.newNews)).Returns(testNews);

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.CreateAsync(testNews)).ReturnsAsync(testNews);
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<NewsDto>(It.IsAny<NewsEntity>())).Returns(testNewsDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(testNewsDto, result.Value);
            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.CreateAsync(testNews), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ThrowsCustomException_WhenNewsIsNull()
        {
            // Arrange
            var testNewsDTO = new NewsDto();
            var command = new CreateNewsCommand(testNewsDTO);
            const string errorMsg = "Cannot convert null to news";

            _mapperMock.Setup(m => m.Map<NewsEntity>(command.newNews)).Returns((NewsEntity)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal(errorMsg, exception.Message);
            Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
            _mapperMock.Verify(m => m.Map<NewsEntity>(command.newNews), Times.Once);
        }

        [Fact]
        public async Task Handle_ThrowsCustomException_WhenSaveChangesFails()
        {
            // Arrange
            var testNews = new NewsEntity { Id = 1, Title = "Test News" };
            var testNewsDto = new NewsDto { Id = 1, Title = "Test News" };
            var command = new CreateNewsCommand(testNewsDto);
            const string errorMsg = "Failed to create a news";

            _mapperMock.Setup(m => m.Map<NewsEntity>(command.newNews)).Returns(testNews);
            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.CreateAsync(testNews)).ReturnsAsync(testNews);
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal(errorMsg, exception.Message);
            Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.CreateAsync(testNews), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<NewsDto>(It.IsAny<NewsEntity>()), Times.Never);
        }
    }
}
