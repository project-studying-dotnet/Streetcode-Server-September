using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;


namespace Streetcode.XUnitTest.MediatRTests.Media.Art
{
    public class DeleteArtHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeleteArtHandler _handler;

        public DeleteArtHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new DeleteArtHandler(_repositoryWrapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnSuccess_WhenArtFound()
        {
            var artId = 1;
            var command = new DeleteArtCommand(artId);
            var art = new ArtEntity() { Id = 1, Title = "Test Art" };

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository
                .GetFirstOrDefaultAsync(It.Is<Expression<Func<ArtEntity, bool>>>(exp => exp.Compile().Invoke(art)),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>())).ReturnsAsync(art);

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.Delete(art));
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(repo => repo.ArtRepository.Delete(art), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnSuccess_WhenArtHasImage()
        {
            var artId = 1;
            var command = new DeleteArtCommand(artId);
            var image = new ImageEntity() { Id = 1, BlobName = "image1" };
            var art = new ArtEntity() { Id = 1, Title = "Test Art", Image = image };

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository
                .GetFirstOrDefaultAsync(It.Is<Expression<Func<ArtEntity, bool>>>(exp => exp.Compile().Invoke(art)),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>())).ReturnsAsync(art);

            _repositoryWrapperMock.Setup(repo => repo.ImageRepository.Delete(art.Image));
            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.Delete(art));
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryWrapperMock.Verify(repo => repo.ArtRepository.Delete(art), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.ImageRepository.Delete(It.Is<ImageEntity>(img => img.Id == 1)), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnFail_WhenArtNotFound()
        {
            var artId = 1;
            var command = new DeleteArtCommand(artId);
            var expectedErrorMsg = $"Cannot find an art by entered Id: {artId}";

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ArtEntity, bool>>>(), null))
                .ReturnsAsync((ArtEntity)null!);

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedErrorMsg, result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(command, expectedErrorMsg), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.ArtRepository.Delete(It.IsAny<ArtEntity>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnFail_WhenSaveChangesFailed()
        {
            // Arrange
            var artId = 1;
            var command = new DeleteArtCommand(artId);
            var expectedErrorMsg = "Failed to delete an art";
            var art = new ArtEntity() { Id = 1, Title = "Test Art" };

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository
                .GetFirstOrDefaultAsync(It.Is<Expression<Func<ArtEntity, bool>>>(exp => exp.Compile().Invoke(art)),
                It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>())).ReturnsAsync(art);

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedErrorMsg, result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(command, expectedErrorMsg), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.ArtRepository.Delete(art), Times.Once);
        }
    }
}
