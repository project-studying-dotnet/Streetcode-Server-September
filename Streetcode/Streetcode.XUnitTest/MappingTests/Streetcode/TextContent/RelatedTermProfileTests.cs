using AutoMapper;
using FluentAssertions;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;
using Streetcode.BLL.Mapping.Streetcode.TextContent;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Xunit;

namespace Streetcode.XUnitTest.MappingTests.Streetcode.TextContent;

public class RelatedTermProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _config;

    public RelatedTermProfileTests()
    {
        _config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RelatedTermProfile>();
            cfg.AddProfile<TermProfile>();
        });

        _mapper = _config.CreateMapper();
    }

    [Fact]
    public void MappingConfiguration_IsValid()
    {
        _config.AssertConfigurationIsValid();
    }
    
    [Fact]
    public void RelatedTerm_ShouldMapTo_RelatedTermDto()
    {
        // Arrange
        var relatedTerm = new RelatedTerm
        {
            Id = 1,
            Word = "SampleWord",
            TermId = 2
        };

        // Act
        var relatedTermDto = _mapper.Map<RelatedTermDto>(relatedTerm);

        // Assert
        Assert.Equal(relatedTerm.Id, relatedTermDto.Id);
        Assert.Equal(relatedTerm.Word, relatedTermDto.Word);
        Assert.Equal(relatedTerm.TermId, relatedTermDto.TermId);
    }

    [Fact]
    public void RelatedTermCreateDto_ShouldMapTo_RelatedTerm()
    {
        // Arrange
        var createDto = new RelatedTermCreateDto("word", 1);

        // Act
        var relatedTerm = _mapper.Map<RelatedTerm>(createDto);

        // Assert
        Assert.Equal(createDto.Word, relatedTerm.Word);
        Assert.Equal(createDto.TermId, relatedTerm.TermId);
    }

    [Fact]
    public void Should_Map_RelatedTerm_To_RelatedTermFullDto()
    {
        // Arrange
        var relatedTerm = new RelatedTerm
        {
            Id = 1,
            Word = "SampleWord",
            Term = new Term { Id = 2, Title = "SampleTerm", Description = "Description"}
        };

        // Act
        var relatedTermFullDto = _mapper.Map<RelatedTermFullDto>(relatedTerm);

        // Assert
        relatedTermFullDto.Id.Should().Be(1);
        relatedTermFullDto.Word.Should().Be("SampleWord");
        relatedTermFullDto.TermDto.Id.Should().Be(2);
        relatedTermFullDto.TermDto.Title.Should().Be("SampleTerm");
    }
}
