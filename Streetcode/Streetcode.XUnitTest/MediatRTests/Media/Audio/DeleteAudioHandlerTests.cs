using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Audio.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

namespace Streetcode.XUnitTest.MediatRTests.Media.Audio;

public class DeleteAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly DeleteAudioHandler _handler;

    public DeleteAudioHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _handler = new DeleteAudioHandler(_repositoryWrapperMock.Object, _blobAzureServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDeletionIsSuccessful()
    {
        // Arrange
        var audioEntity = new AudioEntity { Id = 1, BlobName = "test-audio.mp3" };
        var command = new DeleteAudioCommand(1);

        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()))
            .ReturnsAsync(audioEntity);

        _repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.Delete(audioEntity), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        _blobAzureServiceMock.Verify(service => service.DeleteFileInStorage(audioEntity.BlobName), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenAudioNotFound()
    {
        // Arrange
        var command = new DeleteAudioCommand(1);

        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                 It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                 It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()))
            .ReturnsAsync((AudioEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.Delete(It.IsAny<AudioEntity>()), Times.Never);
        _blobAzureServiceMock.Verify(service => service.DeleteFileInStorage(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenDeletionFails()
    {
        // Arrange
        var audioEntity = new AudioEntity { Id = 1, BlobName = "test-audio.mp3" };
        var command = new DeleteAudioCommand(1);

        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()))
            .ReturnsAsync(audioEntity);

        _repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0); 

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.Delete(audioEntity), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        _blobAzureServiceMock.Verify(service => service.DeleteFileInStorage(It.IsAny<string>()), Times.Never);
    }
}
