using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;
using Streetcode.BLL.Dto.AdditionalContent.Tag;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.Types;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specification.Media.Image;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Create;

public class CreateStreetcodeHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateStreetcodeHandler _handler;
    private readonly Mock<IStringLocalizer<ErrorMessages>> _stringLocalizerMock;

    public CreateStreetcodeHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _stringLocalizerMock = new Mock<IStringLocalizer<ErrorMessages>>();

        _handler = new CreateStreetcodeHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object,
            _stringLocalizerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateStreetcode_WhenValidRequest()
    {
        // Arrange
        var streetcodeCreateDto = new StreetcodeCreateDto
        {
            StreetcodeType = StreetcodeType.Event,
            BlackAndWhiteImageDto = new ImageDto { Id = 1 },
            HistoryLinksImageDto = new ImageDto { Id = 2 },
            GifDto = new ImageDto { Id = 3 },
            Tags = new List<TagShortDto> { new () { Id = 1 } },
            BriefDescription = "desc"
        };
        var command = new CreateStreetcodeCommand(streetcodeCreateDto);
        var createdStreetcode = new EventStreetcode { Id = 1 };

        _mapperMock.Setup(m => m.Map<EventStreetcode>(streetcodeCreateDto)).Returns(createdStreetcode);
        _repositoryWrapperMock.Setup(r => r.ImageRepository.GetItemsBySpecAsync(It.IsAny<GetStreetcodeImagesSpec>()))
            .ReturnsAsync(new List<Image> { new Image(), new Image(), new Image() });
        _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.CreateAsync(It.IsAny<StreetcodeContent>()))
            .ReturnsAsync(createdStreetcode);
        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);

        _repositoryWrapperMock.Verify(r => r.StreetcodeRepository.CreateAsync(It.IsAny<StreetcodeContent>()), Times.Once);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenStreetcodeTypeIsInvalid()
    {
        // Arrange
        var streetcodeCreateDto = new StreetcodeCreateDto
        {
            StreetcodeType = (StreetcodeType)99
        };
        var command = new CreateStreetcodeCommand(streetcodeCreateDto);

        // Act
        var exception = 
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        exception.Message.Should().Be("Streetcode type is unappropriated!");
        exception.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenSaveChangesFails()
    {
        // Arrange
        const string errorMessage = "Save Changes failed";
        var streetcodeCreateDto = new StreetcodeCreateDto
        {
            StreetcodeType = StreetcodeType.Event,
            BlackAndWhiteImageDto = new ImageDto { Id = 1 },
            Tags = new List<TagShortDto> { new () { Id = 1 } }
        };
        var command = new CreateStreetcodeCommand(streetcodeCreateDto);
        var createdStreetcode = new EventStreetcode { Id = 1 };

        _mapperMock.Setup(m => m.Map<EventStreetcode>(streetcodeCreateDto)).Returns(createdStreetcode);
        _repositoryWrapperMock.Setup(r => r.ImageRepository.GetItemsBySpecAsync(It.IsAny<GetStreetcodeImagesSpec>()))
            .ReturnsAsync(new List<Image> { new () });
        _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.CreateAsync(It.IsAny<StreetcodeContent>()))
            .ReturnsAsync(createdStreetcode);
        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);
        
        _stringLocalizerMock
            .Setup(s => s[ErrorKeys.SaveChangesError])
            .Returns(new LocalizedString(ErrorKeys.SaveChangesError, errorMessage));

        // Act 
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        exception.Message.Should().Be(errorMessage);
        exception.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
