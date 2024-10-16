using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Text.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Text;

using Text = DAL.Entities.Streetcode.TextContent.Text;

public class CreateTextHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateTextHandler _handler;

    public CreateTextHandlerTests()
    {
        _repositoryMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateTextHandler(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenTextIsNotMapped()
    {
        // Arrange
        var textCreateDto = new TextCreateDto();
        var command = new CreateTextCommand(textCreateDto);

        _mapperMock
            .Setup(mapper => mapper.Map<Text>(It.IsAny<TextCreateDto>()))
            .Returns((Text)null!);
        
        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Error while mapping TextCreateDto to Text", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.TextRepository.CreateAsync(It.IsAny<Text>()), Times.Never);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenChangesNotSaved()
    {
        // Arrange
        var textCreateDto = new TextCreateDto();
        var command = new CreateTextCommand(textCreateDto);
        var text = new Text();

        _mapperMock
            .Setup(mapper => mapper.Map<Text>(It.IsAny<TextCreateDto>()))
            .Returns(text);

        _repositoryMock
            .Setup(repo => repo.TextRepository.CreateAsync(It.IsAny<Text>()))
            .ReturnsAsync(text);
        _repositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0);
        
        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("No changes saved", exception.Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.TextRepository.CreateAsync(It.IsAny<Text>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenTextDtoNotMapped()
    {
        // Arrange
        var textCreateDto = new TextCreateDto();
        var command = new CreateTextCommand(textCreateDto);
        var text = new Text();

        _mapperMock
            .Setup(mapper => mapper.Map<Text>(It.IsAny<TextCreateDto>()))
            .Returns(text);
        _mapperMock
            .Setup(mapper => mapper.Map<TextDto>(It.IsAny<Text>()))
            .Returns((TextDto)null!);

        _repositoryMock
            .Setup(repo => repo.TextRepository.CreateAsync(It.IsAny<Text>()))
            .ReturnsAsync(text);
        _repositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);
        
        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Error while mapping Text to TextDto", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.TextRepository.CreateAsync(It.IsAny<Text>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnTextDto_WhenChangeSavedAndTextDtoMapped()
    {
        // Arrange
        var textCreateDto = new TextCreateDto();
        var command = new CreateTextCommand(textCreateDto);
        var text = new Text();
        var textDto = new TextDto();

        _mapperMock
            .Setup(mapper => mapper.Map<Text>(It.IsAny<TextCreateDto>()))
            .Returns(text);
        _mapperMock
            .Setup(mapper => mapper.Map<TextDto>(It.IsAny<Text>()))
            .Returns(textDto);

        _repositoryMock
            .Setup(repo => repo.TextRepository.CreateAsync(It.IsAny<Text>()))
            .ReturnsAsync(text);
        _repositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(textDto);
        _repositoryMock.Verify(repo => repo.TextRepository.CreateAsync(It.IsAny<Text>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}