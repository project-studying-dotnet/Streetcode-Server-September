using FluentAssertions;
using MediatR;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Sources.StreetcodeCategoryContent;

using AutoMapper;
using Moq;
using StreetcodeCategoryContent = DAL.Entities.Sources.StreetcodeCategoryContent;

public class CreateStreetcodeCategoryContentHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateStreetcodeCategoryContentHandler _handler;

    public CreateStreetcodeCategoryContentHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateStreetcodeCategoryContentHandler(
            _repositoryWrapperMock.Object, 
            _mapperMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenCategoryContentIsAdded()
    {
        // Assert
        var categoryContentCreateDto = new CategoryContentCreateDto
        {
            Text = "Test",
            StreetcodeId = 1,
            SourceLinkCategoryId = 1,
        };
        var categoryContent = new StreetcodeCategoryContent
        {
            Text = categoryContentCreateDto.Text,
            StreetcodeId = categoryContentCreateDto.StreetcodeId,
            SourceLinkCategoryId = categoryContentCreateDto.SourceLinkCategoryId
        };
        var command = new CreateStreetcodeCategoryContentCommand(categoryContentCreateDto);

        _mapperMock.Setup(x => x.Map<StreetcodeCategoryContent>(categoryContentCreateDto)).Returns(categoryContent);
        _repositoryWrapperMock
            .Setup(x => x.StreetcodeCategoryContentRepository.CreateAsync(It.IsAny<StreetcodeCategoryContent>()))
            .ReturnsAsync(categoryContent);
        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        _repositoryWrapperMock.Verify(
            x => x.StreetcodeCategoryContentRepository.CreateAsync(It.IsAny<StreetcodeCategoryContent>()), 
            Times.Once);
    }
    
    [Fact]
    public void Handle_ShouldReturnFailureResult_WhenDtoNotMapped()
    {
        // Assert
        var categoryContentCreateDto = new CategoryContentCreateDto
        {
            Text = "Test",
            StreetcodeId = 1,
            SourceLinkCategoryId = 1,
        };
        StreetcodeCategoryContent categoryContent = null!;
        var command = new CreateStreetcodeCategoryContentCommand(categoryContentCreateDto);

        _mapperMock.Setup(x => x.Map<StreetcodeCategoryContent>(categoryContentCreateDto)).Returns(categoryContent);

        // Assert
        Assert.Throws<AggregateException>(() => _handler.Handle(command, CancellationToken.None).Result);
        _repositoryWrapperMock.Verify(
            x => x.StreetcodeCategoryContentRepository.CreateAsync(It.IsAny<StreetcodeCategoryContent>()), 
            Times.Never);
    }
    
    [Fact]
    public void Handle_ShouldReturnFailureResult_WhenChangesNotSaved()
    {
        // Assert
        var categoryContentCreateDto = new CategoryContentCreateDto
        {
            Text = "Test",
            StreetcodeId = 1,
            SourceLinkCategoryId = 1,
        };
        var categoryContent = new StreetcodeCategoryContent
        {
            Text = categoryContentCreateDto.Text,
            StreetcodeId = categoryContentCreateDto.StreetcodeId,
            SourceLinkCategoryId = categoryContentCreateDto.SourceLinkCategoryId
        };
        var command = new CreateStreetcodeCategoryContentCommand(categoryContentCreateDto);

        _mapperMock.Setup(x => x.Map<StreetcodeCategoryContent>(categoryContentCreateDto)).Returns(categoryContent);
        _repositoryWrapperMock
            .Setup(x => x.StreetcodeCategoryContentRepository.CreateAsync(It.IsAny<StreetcodeCategoryContent>()))
            .ReturnsAsync(categoryContent);
        _repositoryWrapperMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

        // Assert
        Assert.Throws<AggregateException>(() => _handler.Handle(command, CancellationToken.None).Result);
        _repositoryWrapperMock.Verify(
            x => x.StreetcodeCategoryContentRepository.CreateAsync(It.IsAny<StreetcodeCategoryContent>()), 
            Times.Once);
    }
}