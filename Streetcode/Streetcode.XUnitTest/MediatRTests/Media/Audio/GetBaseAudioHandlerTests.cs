using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Audio.GetBaseAudio;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using AudioEntity = Streetcode.DAL.Entities.Media.Audio;
namespace Streetcode.XUnitTest.MediatRTests.Media.Audio;

public class GetBaseAudioHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly GetBaseAudioHandler _handler;

    public GetBaseAudioHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _handler = new GetBaseAudioHandler(_blobAzureServiceMock.Object, _repositoryWrapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMemoryStream_WhenAudioIsFound()
    {
        // Arrange
        var audioEntity = new AudioEntity
        {
            Id = 1,
            BlobName = "audio1.mp3"
        };

        var memoryStream = new MemoryStream();

        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()))
            .ReturnsAsync(audioEntity);

        _blobAzureServiceMock
            .Setup(service => service.FindFileInStorageAsMemoryStream(It.IsAny<string>()))
            .Returns(memoryStream);

        // Act
        var result = await _handler.Handle(new GetBaseAudioQuery(audioEntity.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(memoryStream, result.Value);

        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsMemoryStream(audioEntity.BlobName), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenAudioNotFound()
    {
        // Arrange
        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()))
            .ReturnsAsync((AudioEntity)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(new GetBaseAudioQuery(1), CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        Assert.Equal("Cannot find an audio with corresponding id: 1", exception.Message);

        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsMemoryStream(It.IsAny<string>()), Times.Never);
    }
}