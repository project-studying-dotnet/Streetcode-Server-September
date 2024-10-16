using AutoMapper;
using FluentAssertions;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.Mapping.Streetcode.TextContent;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Xunit;

namespace Streetcode.XUnitTest.MappingTests.Streetcode.TextContent;

public class TextProfileTests
{
    private readonly IMapper _mapper;

    public TextProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TextProfile>();
        });

        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Text_ShouldMapTo_TextDto()
    {
        // Arrange
        var text = new Text
        {
            Id = 1,
            Title = "Title",
            TextContent = "TextContent",
            AdditionalText = "AdditionalText",
            StreetcodeId = 1,
        };
        
        // Act
        var textDto = _mapper.Map<TextDto>(text);
        
        // Assert
        textDto.Id.Should().Be(text.Id);
        textDto.Title.Should().Be(text.Title);
        textDto.TextContent.Should().Be(text.TextContent);
        textDto.AdditionalText.Should().Be(text.AdditionalText);
        textDto.StreetcodeId.Should().Be(text.StreetcodeId);
    }

    [Fact]
    public void TextDto_ShouldMapTo_Text()
    {
        // Arrange
        var textDto = new TextDto
        {
            Id = 1,
            Title = "Title",
            TextContent = "TextContent",
            AdditionalText = "AdditionalText",
            StreetcodeId = 1,
        };
        
        // Act
        var text = _mapper.Map<Text>(textDto);
        
        // Assert
        text.Id.Should().Be(textDto.Id);
        text.Title.Should().Be(textDto.Title);
        text.TextContent.Should().Be(textDto.TextContent);
        text.AdditionalText.Should().Be(textDto.AdditionalText);
        text.StreetcodeId.Should().Be(textDto.StreetcodeId);
    }

    [Fact]
    public void TextCreateDto_ShouldMapTo_Text()
    {
        // Arrange
        var textDto = new TextCreateDto
        {
            Title = "Title",
            TextContent = "TextContent",
            AdditionalText = "AdditionalText",
            StreetcodeId = 1,
        };
        
        // Act
        var text = _mapper.Map<Text>(textDto);
        
        // Assert
        text.Title.Should().Be(textDto.Title);
        text.TextContent.Should().Be(textDto.TextContent);
        text.AdditionalText.Should().Be(textDto.AdditionalText);
        text.StreetcodeId.Should().Be(textDto.StreetcodeId);
    }
}