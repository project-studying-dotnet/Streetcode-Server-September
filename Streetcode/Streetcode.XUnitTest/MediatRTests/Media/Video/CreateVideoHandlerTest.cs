using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Dto.Media.Video;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Media.Video.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Media.Video;

using Video = DAL.Entities.Media.Video;

public class CreateVideoHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRepositoryWrapper> _repositoryMock;
    private readonly CreateVideoHandler _handler;

    public CreateVideoHandlerTest()
    {
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IRepositoryWrapper>();
        _handler = new CreateVideoHandler(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenCreateDtoNotMapped()
    {
        // Arrange
        var videoCreateDto = new VideoCreateDto();
        var command = new CreateVideoCommand(videoCreateDto);

        _mapperMock
            .Setup(mapper => mapper.Map<Video>(It.IsAny<VideoCreateDto>()))
            .Returns((Video)null!);

        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Can not map dto to entity", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.VideoRepository.CreateAsync(It.IsAny<Video>()), Times.Never);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenChangesNotSaved()
    {
        // Arrange
        var videoCreateDto = new VideoCreateDto();
        var command = new CreateVideoCommand(videoCreateDto);
        var video = new Video();

        _mapperMock
            .Setup(mapper => mapper.Map<Video>(It.IsAny<VideoCreateDto>()))
            .Returns(video);

        _repositoryMock
            .Setup(repo => repo.VideoRepository.CreateAsync(It.IsAny<Video>()))
            .ReturnsAsync(video);
        _repositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Can not create video", exception.Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.VideoRepository.CreateAsync(It.IsAny<Video>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenVideoNotMapped()
    {
        // Arrange
        var videoCreateDto = new VideoCreateDto();
        var command = new CreateVideoCommand(videoCreateDto);
        var video = new Video();
        
        _mapperMock
            .Setup(mapper => mapper.Map<Video>(It.IsAny<VideoCreateDto>()))
            .Returns(video);
        _mapperMock
            .Setup(mapper => mapper.Map<VideoDto>(It.IsAny<Video>()))
            .Returns((VideoDto)null!);

        _repositoryMock
            .Setup(repo => repo.VideoRepository.CreateAsync(It.IsAny<Video>()))
            .ReturnsAsync(video);
        _repositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Can not map entity to dto", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.VideoRepository.CreateAsync(It.IsAny<Video>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnCreatedVideoDto_WhenVideoCreatedAndMapped()
    {
        // Arrange
        var videoCreateDto = new VideoCreateDto();
        var command = new CreateVideoCommand(videoCreateDto);
        var video = new Video();
        var videoDto = new VideoDto();
        
        _mapperMock
            .Setup(mapper => mapper.Map<Video>(It.IsAny<VideoCreateDto>()))
            .Returns(video);
        _mapperMock
            .Setup(mapper => mapper.Map<VideoDto>(It.IsAny<Video>()))
            .Returns(videoDto);

        _repositoryMock
            .Setup(repo => repo.VideoRepository.CreateAsync(It.IsAny<Video>()))
            .ReturnsAsync(video);
        _repositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(videoDto);
        _repositoryMock.Verify(repo => repo.VideoRepository.CreateAsync(It.IsAny<Video>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}