using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Audio.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using AudioEntity = Streetcode.DAL.Entities.Media.Audio;
namespace Streetcode.XUnitTest.MediatRTests.Media.Audio;

public class GetAudioByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly GetAudioByIdHandler _handler;

    public GetAudioByIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _handler = new GetAudioByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobAzureServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAudioDto_WhenAudioIsFound()
    {
        // Arrange
        var audioEntity = new AudioEntity
        {
            Id = 1,
            BlobName = "audio1.mp3"
        };

        var audioDto = new AudioDto
        {
            Id = 1,
            BlobName = "audio1.mp3"
        };

        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()))
            .ReturnsAsync(audioEntity);

        _mapperMock
            .Setup(m => m.Map<AudioDto>(audioEntity))
            .Returns(audioDto);

        _blobAzureServiceMock
            .Setup(service => service.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns("base64string");

        // Act
        var result = await _handler.Handle(new GetAudioByIdQuery(1), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(audioDto.Id, result.Value.Id);
        Assert.Equal("base64string", result.Value.Base64);

        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()), Times.Once);
        _mapperMock.Verify(m => m.Map<AudioDto>(audioEntity), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64(audioDto.BlobName), Times.Once);
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
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(new GetAudioByIdQuery(1), CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        Assert.Equal("Cannot find an audio with corresponding id: 1", exception.Message);

        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.GetFirstOrDefaultAsync(
                                    It.IsAny<Expression<Func<AudioEntity, bool>>>(),
                It.IsAny<Func<IQueryable<AudioEntity>, IIncludableQueryable<AudioEntity, object>>>()), Times.Once);
        _mapperMock.Verify(m => m.Map<AudioDto>(It.IsAny<AudioEntity>()), Times.Never);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
    }
}
