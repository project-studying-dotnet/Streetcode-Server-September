using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatRTests.News
{
    public class UpdateNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly UpdateNewsHandler _handler;

        public UpdateNewsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new UpdateNewsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenNewsIsUpdatedSuccessfully()
        {
            // Arrange
            var news = new NewsEntity { Id = 1, Title = "Test Title", Image = new Image { BlobName = "image1" } };
            var newsDto = new NewsDto { Id = 1, Title = "Test Title", Image = new ImageDto { BlobName = "image1" } };
            var responseNewsDto = new NewsDto { Id = 1, Title = "Updated Title", Image = new ImageDto { BlobName = "image1" } };
            var command = new UpdateNewsCommand(newsDto);

            _mapperMock.Setup(m => m.Map<NewsEntity>(command.news)).Returns(news);
            _mapperMock.Setup(m => m.Map<NewsDto>(news)).Returns(responseNewsDto);

            _blobServiceMock.Setup(blob => blob.FindFileInStorageAsBase64("image1"))
                .Returns("base64image");

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.Update(news));
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(responseNewsDto.Id, result.Value.Id);
            Assert.Equal(responseNewsDto.Title, result.Value.Title);
            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.Update(news), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenNewsIsNull()
        {
            // Arrange
            var newsDto = new NewsDto();
            var command = new UpdateNewsCommand(newsDto);
            string errorMsg = "Cannot convert null to news";

            _mapperMock.Setup(m => m.Map<NewsEntity>(command.news)).Returns((NewsEntity)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(errorMsg, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenSaveChangesFails()
        {
            // Arrange
            var news = new NewsEntity { Id = 1, Image = new Image { BlobName = "image1" } };
            var newsDto = new NewsDto { Id = 1, Image = new ImageDto { BlobName = "image1" } };
            var responseNewsDto = new NewsDto { Id = 1, Image = new ImageDto { BlobName = "image1" } };
            var command = new UpdateNewsCommand(newsDto);
            string errorMsg = "Failed to update news";

            _mapperMock.Setup(m => m.Map<NewsEntity>(command.news)).Returns(news);
            _mapperMock.Setup(m => m.Map<NewsDto>(news)).Returns(responseNewsDto);

            _blobServiceMock.Setup(blob => blob.FindFileInStorageAsBase64("image1"))
                .Returns("base64image");

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository.Update(news));
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMsg, result.Errors.First().Message);
            _repositoryWrapperMock.Verify(repo => repo.NewsRepository.Update(news), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<NewsDto>(It.IsAny<NewsEntity>()), Times.Once);
            _mapperMock.Verify(m => m.Map<NewsEntity>(It.IsAny<NewsDto>()), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }
    }
}
