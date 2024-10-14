using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Audio.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

namespace Streetcode.XUnitTest.MediatRTests.Media.Audio;

public class GetAllAudiosHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly GetAllAudiosHandler _handler;

    public GetAllAudiosHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _handler = new GetAllAudiosHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobAzureServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAudios_WhenAudiosAreFound()
    {
        // Arrange
        var audioEntities = new List<AudioEntity>
        {
            new AudioEntity { Id = 1, BlobName = "audio1.mp3" },
            new AudioEntity { Id = 2, BlobName = "audio2.mp3" }
        };

        var audioDtos = new List<AudioDto>
        {
            new AudioDto { Id = 1, BlobName = "audio1.mp3" },
            new AudioDto { Id = 2, BlobName = "audio2.mp3" }
        };

        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository
            .GetAllAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()))
            .ReturnsAsync(audioEntities);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<AudioDto>>(audioEntities))
            .Returns(audioDtos);

        _blobAzureServiceMock
            .Setup(service => service.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns("base64string");

        // Act
        var result = await _handler.Handle(new GetAllAudiosQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(audioDtos.Count, result.Value.Count());
        Assert.All(result.Value, dto => Assert.Equal("base64string", dto.Base64));

        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.GetAllAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Exactly(audioDtos.Count));
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenNoAudiosFound()
    {
        // Arrange
        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository
            .GetAllAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()))
            .ReturnsAsync((IEnumerable<AudioEntity>?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(new GetAllAudiosQuery(), CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        Assert.Equal("Cannot find any audios", exception.Message);

        _repositoryWrapperMock.Verify(repo => repo.AudioRepository
        .GetAllAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoAudiosInRepository()
    {
        // Arrange
        var audioEntities = new List<AudioEntity>(); // Empty list

        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository.GetAllAsync(
                It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()))
            .ReturnsAsync(audioEntities);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<AudioDto>>(audioEntities))
            .Returns(new List<AudioDto>());

        // Act
        var result = await _handler.Handle(new GetAllAudiosQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);

        _repositoryWrapperMock.Verify(repo => repo.AudioRepository
               .GetAllAsync(It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
    }
}