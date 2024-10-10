using AutoMapper;
using Streetcode.BLL.Dto.Media.Audio;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.Mapping.Media;
using Streetcode.BLL.Mapping.Media.Images;
using Streetcode.BLL.Mapping.Streetcode;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Entities.Streetcode.Types;
using Streetcode.DAL.Enums;
using Xunit;

namespace Streetcode.XUnitTest.MappingTests.Streetcode;

public class StreetcodeProfileTests
{
    private readonly IMapper _mapper;

    public StreetcodeProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<StreetcodeProfile>();
            cfg.AddProfile<ImageProfile>();
            cfg.AddProfile<AudioProfile>();
        });

        _mapper = config.CreateMapper();
    }

    private static StreetcodeContent GetStreetcodeContent() => new ()
    {
        Id = 0,
        Index = 0,
        Teaser = "Teaser",
        DateString = "DateString",
        Alias = "Alias",
        Status = StreetcodeStatus.Draft,
        Title = "Title",
        TransliterationUrl = "TransliterationUrl",
        ViewCount = 0,
        CreatedAt = DateTime.MinValue,
        UpdatedAt = DateTime.MinValue,
        EventStartOrPersonBirthDate = DateTime.MaxValue,
        EventEndOrPersonDeathDate = DateTime.MaxValue,
        AudioId = 0,
        BriefDescription = "BriefDescription",
        Text = new Text { Title = "Text", TextContent = "Text content"},
        Audio = null,
        StatisticRecords = null!,
        Coordinates = null!,
        TransactionLink = null,
        Toponyms = null!,
        Images = new List<Image> { new() { Id = 1 } },
        StreetcodeTagIndices = null!,
        Tags = null!,
        Subtitles = null!,
        Facts = null!,
        Videos = null!,
        SourceLinkCategories = null!,
        TimelineItems = null!,
        Observers = null!,
        Targets = null!,
        Partners = null!,
        StreetcodeArts = null!,
        StreetcodeCategoryContents = null!,
        Comments = null!,
    };
    
    [Fact]
    public void StreetcodeContent_To_StreetcodeDto_Should_Map_Properly()
    {
        // Arrange
        var streetcodeContent = GetStreetcodeContent();

        // Act
        var result = _mapper.Map<StreetcodeDto>(streetcodeContent);

        // Assert
        Assert.Equal(streetcodeContent.Id, result.Id);
        Assert.Equal(streetcodeContent.Index, result.Index);
        Assert.Equal(streetcodeContent.Teaser, result.Teaser);
        Assert.Equal(streetcodeContent.DateString, result.DateString);
        Assert.Equal(streetcodeContent.Alias, result.Alias);
        Assert.Equal(streetcodeContent.Status, result.Status);
        Assert.Equal(streetcodeContent.Title, result.Title);
        Assert.Equal(streetcodeContent.TransliterationUrl, result.TransliterationUrl);
        Assert.Equal(streetcodeContent.ViewCount, result.ViewCount);
        Assert.Equal(streetcodeContent.CreatedAt, result.CreatedAt);
        Assert.Equal(streetcodeContent.UpdatedAt, result.UpdatedAt);
        Assert.Equal(streetcodeContent.EventStartOrPersonBirthDate, result.EventStartOrPersonBirthDate);
        Assert.Equal(streetcodeContent.EventEndOrPersonDeathDate, result.EventEndOrPersonDeathDate);
    }

    [Fact]
    public void StreetcodeContent_To_StreetcodeShortDto_Should_Map_Properly()
    {
        // Arrange
        var streetcodeContent = GetStreetcodeContent();

        // Act
        var result = _mapper.Map<StreetcodeShortDto>(streetcodeContent);

        // Assert
        Assert.Equal(streetcodeContent.Id, result.Id);
        Assert.Equal(streetcodeContent.Title, result.Title);
    }

    [Fact]
    public void StreetcodeContent_To_StreetcodeMainPageDto_Should_Map_Properly()
    {
        // Arrange
        var streetcodeContent = GetStreetcodeContent();

        // Act
        var result = _mapper.Map<StreetcodeMainPageDto>(streetcodeContent);

        // Assert
        Assert.Equal(streetcodeContent.Text!.Title, result.Text);
        Assert.Equal(streetcodeContent.Images.LastOrDefault()!.Id, result.ImageId);
    }

    [Fact]
    public void StreetcodeMainBlockCreateDto_To_StreetcodeCreateDto_Should_Map_Properly()
    {
        // Arrange
        var createDto = new StreetcodeMainBlockCreateDto
        {
            Index = 0,
            StreetcodeType = StreetcodeType.Event,
            Title = "null",
            FirstName = "null",
            Rank = "null",
            LastName = "null",
            EventStartOrPersonBirthDate = DateTime.MinValue,
            EventEndOrPersonDeathDate = DateTime.MinValue,
            Tags = null,
            Teaser = "null",
            TransliterationUrl = "null",
            BriefDescription = "null",
            AudioFileBaseCreate = new AudioFileBaseCreateDto(),
            BlackAndWhiteImageFileBaseCreateDto = new ImageFileBaseCreateDto(),
            HistoryLinksImageFileBaseCreateDto = new ImageFileBaseCreateDto(),
            GifFileBaseCreateDto = new ImageFileBaseCreateDto()
        };

        // Act
        var result = _mapper.Map<StreetcodeCreateDto>(createDto);

        // Assert
        Assert.Equal(createDto.Index, result.Index);
        Assert.Equal(createDto.StreetcodeType, result.StreetcodeType);
        Assert.Equal(createDto.Title, result.Title);
        Assert.Equal(createDto.FirstName, result.FirstName);
        Assert.Equal(createDto.Rank, result.Rank);
        Assert.Equal(createDto.LastName, result.LastName);
        Assert.Equal(createDto.EventStartOrPersonBirthDate, result.EventStartOrPersonBirthDate);
        Assert.Equal(createDto.EventEndOrPersonDeathDate, result.EventEndOrPersonDeathDate);
        Assert.Equal(createDto.Teaser, result.Teaser);
        Assert.Equal(createDto.TransliterationUrl, result.TransliterationUrl);
        Assert.Equal(createDto.BriefDescription, result.BriefDescription);
    
        Assert.Null(result.AudioDto);
        Assert.Null(result.BlackAndWhiteImageDto);
        Assert.Null(result.HistoryLinksImageDto);
        Assert.Null(result.GifDto);
    }

    [Fact]
    public void StreetcodeCreateDto_To_StreetcodeContent_Should_Map_Properly()
    {
        // Arrange
        var createDto = new StreetcodeCreateDto
        {
            EventStartOrPersonBirthDate = new DateTime(2020, 1, 1),
            EventEndOrPersonDeathDate = new DateTime(2020, 12, 31)
        };

        // Act
        var result = _mapper.Map<StreetcodeContent>(createDto);

        // Assert
        Assert.NotNull(result.DateString);
        Assert.Equal("01 січня 2020 року - 31 грудня 2020 року", result.DateString);
    }

    [Fact]
    public void StreetcodeCreateDto_To_PersonStreetcode_Should_Map_Properly()
    {
        // Arrange
        var createDto = new StreetcodeCreateDto
        {
            Index = 0,
            StreetcodeType = StreetcodeType.Event,
            Title = "null",
            FirstName = "null",
            Rank = "null",
            LastName = "null",
            EventStartOrPersonBirthDate = DateTime.MaxValue,
            EventEndOrPersonDeathDate = DateTime.MaxValue,
            Tags = null,
            Teaser = "null",
            TransliterationUrl = "null",
            BriefDescription = "null",
            AudioDto = new AudioDto(),
            BlackAndWhiteImageDto = new ImageDto(),
            HistoryLinksImageDto = new ImageDto(),
            GifDto = new ImageDto()
        };

        // Act
        var result = _mapper.Map<PersonStreetcode>(createDto);

        // Assert
        Assert.IsType<PersonStreetcode>(result);
        Assert.Equal(createDto.Index, result.Index);
        Assert.Equal(createDto.Title, result.Title);
        Assert.Equal(createDto.FirstName, result.FirstName);
        Assert.Equal(createDto.Rank, result.Rank);
        Assert.Equal(createDto.LastName, result.LastName);
        Assert.Equal(createDto.EventStartOrPersonBirthDate, result.EventStartOrPersonBirthDate);
        Assert.Equal(createDto.EventEndOrPersonDeathDate, result.EventEndOrPersonDeathDate);
        Assert.Equal(createDto.Teaser, result.Teaser);
        Assert.Equal(createDto.TransliterationUrl, result.TransliterationUrl);
        Assert.Equal(createDto.BriefDescription, result.BriefDescription);
    
        Assert.Null(result.Audio);
        Assert.Empty(result.Images);
    }

    [Fact]
    public void StreetcodeCreateDto_To_EventStreetcode_Should_Map_Properly()
    {
        // Arrange
        var createDto = new StreetcodeCreateDto
        {
            Index = 0,
            StreetcodeType = StreetcodeType.Event,
            Title = "null",
            FirstName = "null",
            Rank = "null",
            LastName = "null",
            EventStartOrPersonBirthDate = DateTime.MaxValue,
            EventEndOrPersonDeathDate = DateTime.MaxValue,
            Tags = null,
            Teaser = "null",
            TransliterationUrl = "null",
            BriefDescription = "null",
            AudioDto = new AudioDto(),
            BlackAndWhiteImageDto = new ImageDto(),
            HistoryLinksImageDto = new ImageDto(),
            GifDto = new ImageDto()
        };

        // Act
        var result = _mapper.Map<EventStreetcode>(createDto);

        // Assert
        Assert.IsType<EventStreetcode>(result);
        Assert.Equal(createDto.Index, result.Index);
        Assert.Equal(createDto.Title, result.Title);
        Assert.Equal(createDto.EventStartOrPersonBirthDate, result.EventStartOrPersonBirthDate);
        Assert.Equal(createDto.EventEndOrPersonDeathDate, result.EventEndOrPersonDeathDate);
        Assert.Equal(createDto.Teaser, result.Teaser);
        Assert.Equal(createDto.TransliterationUrl, result.TransliterationUrl);
        Assert.Equal(createDto.BriefDescription, result.BriefDescription);
    
        Assert.Null(result.Audio);
        Assert.Empty(result.Images);
    }
}
