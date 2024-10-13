using AutoMapper;
using FluentResults;
using MediatR;
using Moq;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.MediatR.Media.Audio.Create;
using Streetcode.BLL.MediatR.Media.Image.Create;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Create;

public class CreateStreetcodeCoordinatorTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateStreetcodeCoordinator _handler;

    public CreateStreetcodeCoordinatorTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateStreetcodeCoordinator(_mediatorMock.Object, _mapperMock.Object);
    }

    public static IEnumerable<object[]> BlobTestData => new List<object[]>
    {
        new object[]
        {
            new ImageFileBaseCreateDto(),
            new AudioFileBaseCreateDto(),
            new ImageFileBaseCreateDto(),
            new ImageFileBaseCreateDto(),
            3,
            1
        },
        new object[]
        {
            new ImageFileBaseCreateDto(),
            null!,
            new ImageFileBaseCreateDto(),
            new ImageFileBaseCreateDto(),
            3,
            0
        },
        new object[]
        {
            new ImageFileBaseCreateDto(),
            new AudioFileBaseCreateDto(),
            new ImageFileBaseCreateDto(),
            null!,
            2,
            1
        },
        new object[]
        {
            new ImageFileBaseCreateDto(),
            new AudioFileBaseCreateDto(),
            null!,
            null!,
            1,
            1
        },
        new object[]
        {
            new ImageFileBaseCreateDto(),
            null!,
            null!,
            null!,
            1,
            0
        },
    };
    
    [Theory]
    [MemberData(nameof(BlobTestData))]
    public async Task Handle_ShouldCreateStreetcodeSuccessfully(
        ImageFileBaseCreateDto blackAndWhiteImage, 
        AudioFileBaseCreateDto audio,
        ImageFileBaseCreateDto historyLinkImage, 
        ImageFileBaseCreateDto gifImage,
        int createImageTimes,
        int createAudioTimes
        )
    {
        // Arrange
        var streetcodeMainBlockCreateDto = new StreetcodeMainBlockCreateDto
        {
            BlackAndWhiteImageFileBaseCreateDto = blackAndWhiteImage,
            AudioFileBaseCreate = audio,
            HistoryLinksImageFileBaseCreateDto = historyLinkImage,
            GifFileBaseCreateDto = gifImage
        };
        var request = new CreateStreetcodeMainBlockCommand(streetcodeMainBlockCreateDto);

        var streetcodeCreateDto = new StreetcodeCreateDto();
        var imageDto = new ImageDto();
        var audioDto = new AudioDto();
        
        // Setup AutoMapper
        _mapperMock.Setup(m => m.Map<StreetcodeCreateDto>(It.IsAny<StreetcodeMainBlockCreateDto>()))
            .Returns(streetcodeCreateDto);

        // Setup MediatR commands
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(imageDto));
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateAudioCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(audioDto));

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateStreetcodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(1));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<CreateImageCommand>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(createImageTimes));
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<CreateAudioCommand>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(createAudioTimes));
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<CreateStreetcodeCommand>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotCreateAudioIfNull()
    {
        // Arrange
        var streetcodeMainBlockCreateDto = new StreetcodeMainBlockCreateDto
        {
            BlackAndWhiteImageFileBaseCreateDto = new ImageFileBaseCreateDto(),
            HistoryLinksImageFileBaseCreateDto = new ImageFileBaseCreateDto(),
            GifFileBaseCreateDto = new ImageFileBaseCreateDto(),
            BriefDescription = "desc" 
        };
        var request = new CreateStreetcodeMainBlockCommand(streetcodeMainBlockCreateDto);

        var streetcodeCreateDto = new StreetcodeCreateDto();
        var imageDto = new ImageDto();
        
        _mapperMock.Setup(m => m.Map<StreetcodeCreateDto>(It.IsAny<StreetcodeMainBlockCreateDto>()))
            .Returns(streetcodeCreateDto);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(imageDto));
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateStreetcodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(1));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);

        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateAudioCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

