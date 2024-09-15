using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.XUnitTest.MediatRTests.Sources.SourceLinkCategory;

using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using MediatR;
using SourceLinkCategory = DAL.Entities.Sources.SourceLinkCategory;

public class CreateSourceLinkCategoryHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerServiceMock;
    private readonly CreateSourceLinkCategoryHandler _handler;

    public CreateSourceLinkCategoryHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerServiceMock = new Mock<ILoggerService>();
        _handler = new CreateSourceLinkCategoryHandler(
            _repositoryWrapperMock.Object, 
            _mapperMock.Object, 
            _loggerServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenSourceLinkIsAdded()
    {
        // Assert
        var sourceLinkCategoryCreateDto = new SourceLinkCategoryCreateDto("Title", 1);
        var command = new CreateSourceLinkCategoryCommand(sourceLinkCategoryCreateDto);
        var sourceLinkCategory = new SourceLinkCategory
        {
            Title = sourceLinkCategoryCreateDto.Title,
            ImageId = sourceLinkCategoryCreateDto.ImageId,
        };

        _mapperMock.Setup(x => x.Map<SourceLinkCategory>(sourceLinkCategoryCreateDto)).Returns(sourceLinkCategory);
        _repositoryWrapperMock
            .Setup(x => x.SourceCategoryRepository.CreateAsync(It.IsAny<SourceLinkCategory>()))
            .ReturnsAsync(sourceLinkCategory);
        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        _repositoryWrapperMock.Verify(
            x => x.SourceCategoryRepository.CreateAsync(It.IsAny<SourceLinkCategory>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenDtoNotMapped()
    {
        // Arrange
        SourceLinkCategoryCreateDto sourceLinkCategoryDto = null!;
        SourceLinkCategory sourceLinkCategory = null!;
        var command = new CreateSourceLinkCategoryCommand(sourceLinkCategoryDto);

        _mapperMock
            .Setup(x => x.Map<SourceLinkCategory>(It.IsAny<SourceLinkCategoryCreateDto>()))
            .Returns(sourceLinkCategory);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsFailed.Should().BeTrue();
        _loggerServiceMock.Verify(
            x => x.LogError(It.IsAny<CreateSourceLinkCategoryCommand>(), "Cannot convert null to SourceLinkCategory"),
            Times.Once);
        _repositoryWrapperMock.Verify(
            x => x.SourceCategoryRepository.CreateAsync(It.IsAny<SourceLinkCategory>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenChangesNotSaved()
    {
        // Assert
        var sourceLinkCategoryCreateDto = new SourceLinkCategoryCreateDto("Title", 1);
        var sourceLinkCategory = new SourceLinkCategory
        {
            Title = sourceLinkCategoryCreateDto.Title,
            ImageId = sourceLinkCategoryCreateDto.ImageId,
        };
        var command = new CreateSourceLinkCategoryCommand(sourceLinkCategoryCreateDto);

        _mapperMock.Setup(x => x.Map<SourceLinkCategory>(sourceLinkCategoryCreateDto)).Returns(sourceLinkCategory);
        _repositoryWrapperMock
            .Setup(x => x.SourceCategoryRepository.CreateAsync(It.IsAny<SourceLinkCategory>()))
            .ReturnsAsync(sourceLinkCategory);
        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert 
        result.IsFailed.Should().BeTrue();
        _loggerServiceMock.Verify(
            x => x.LogError(It.IsAny<CreateSourceLinkCategoryCommand>(), "Failed to create SourceLinkCategory"),
            Times.Once);
    }
}