using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Audio.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using AudioEntity = Streetcode.DAL.Entities.Media.Audio;

namespace Streetcode.XUnitTest.MediatRTests.Media.Audio;

public class CreateAudioHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly CreateAudioHandler _handler;

    public CreateAudioHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _handler = new CreateAudioHandler(_mapperMock.Object, _repositoryWrapperMock.Object, _blobAzureServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAudioDto_WhenCreationIsSuccessful()
    {
        // Arrange
        var audioEntity = new AudioEntity
        {
            Id = 1,
            BlobName = "Test Audio"
        };

        var audioCreateDto = new AudioFileBaseCreateDto
        {
            Title = "Test Audio",
            Description = "Sample description",
            BaseFormat = "mp3",
            MimeType = "audio/mpeg",
            Extension = ".mp3"
        };

        var createdAudioDto = new AudioDto
        {
            Id = 1,
            BlobName = "Test Audio",
            Description = "Sample description",
            MimeType = "audio/mpeg"
        };

        var command = new CreateAudioCommand(audioCreateDto);

        _blobAzureServiceMock
            .Setup(service => service.SaveFileInStorage(audioCreateDto.BaseFormat, audioCreateDto.Title, string.Empty))
            .Verifiable();

        _mapperMock
            .Setup(m => m.Map<AudioEntity>(audioCreateDto))
            .Returns(audioEntity);

        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository.CreateAsync(audioEntity))
            .Verifiable();

        _repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(m => m.Map<AudioDto>(audioEntity))
            .Returns(createdAudioDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createdAudioDto, result.Value);

        _blobAzureServiceMock.Verify(service => service.SaveFileInStorage(audioCreateDto.BaseFormat, audioCreateDto.Title, string.Empty), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.CreateAsync(audioEntity), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        _mapperMock.Verify(m => m.Map<AudioDto>(audioEntity), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenSaveFails()
    {
        // Arrange
        var audioCreateDto = new AudioFileBaseCreateDto
        {
            Title = "Test Audio",
            Description = "Sample description",
            BaseFormat = "mp3",
            MimeType = "audio/mpeg",
            Extension = ".mp3"
        };

        var command = new CreateAudioCommand(audioCreateDto);

        var audioEntity = new AudioEntity
        {
            BlobName = audioCreateDto.Title
        };

        _blobAzureServiceMock
            .Setup(service => service.SaveFileInStorage(audioCreateDto.BaseFormat, audioCreateDto.Title, string.Empty))
            .Verifiable();

        _mapperMock
            .Setup(m => m.Map<AudioEntity>(audioCreateDto))
            .Returns(audioEntity);

        _repositoryWrapperMock
            .Setup(repo => repo.AudioRepository.CreateAsync(It.IsAny<AudioEntity>()))
            .Verifiable();

        _repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0); 

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Failed to create an audio", exception.Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);

        _blobAzureServiceMock.Verify(service => service.SaveFileInStorage(audioCreateDto.BaseFormat, audioCreateDto.Title, string.Empty), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.AudioRepository.CreateAsync(It.IsAny<AudioEntity>()), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}