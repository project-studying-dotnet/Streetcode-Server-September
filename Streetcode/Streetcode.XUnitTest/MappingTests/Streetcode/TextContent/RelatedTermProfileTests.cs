using AutoMapper;
using FluentAssertions;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Xunit;

namespace Streetcode.XUnitTest.MappingTests.Streetcode.TextContent;

public class RelatedTermProfileTests
{
    private readonly IMapper _mapper;

    public RelatedTermProfileTests()
    {
        var config = GetMapperConfiguration();

        _mapper = config.CreateMapper();
    }

    [Fact]
    public void MappingConfiguration_IsValid()
    {
        var config = GetMapperConfiguration();

        config.AssertConfigurationIsValid();
    }
    
    private static MapperConfiguration GetMapperConfiguration() => new (cfg =>
    {
        cfg.CreateMap<Term, TermDto>().ReverseMap();
        cfg.CreateMap<RelatedTerm, RelatedTermDto>().ReverseMap();
        cfg.CreateMap<RelatedTermCreateDto, RelatedTerm>()
            .ForMember(entity => entity.Word, opt => opt.MapFrom(src => src.Word))
            .ForMember(entity => entity.TermId, opt => opt.MapFrom(src => src.TermId))
            .ForMember(entity => entity.Term, opt => opt.MapFrom<Term>(_ => null!))
            .ForMember(entity => entity.Id, opt => opt.MapFrom<int>(_ => default));
        cfg.CreateMap<RelatedTerm, RelatedTermFullDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("Word", opt => opt.MapFrom(src => src.Word))
            .ForCtorParam("TermDto", opt => opt.MapFrom(src => src.Term));
        cfg.CreateMap<RelatedTermFullDto, RelatedTerm>()
            .ForMember(entity => entity.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(entity => entity.Word, opt => opt.MapFrom(src => src.Word))
            .ForMember(entity => entity.Term, opt => opt.MapFrom(src => src.TermDto))
            .ForMember(entity => entity.TermId, opt => opt.MapFrom(src => src.TermDto.Id));
    });
    
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
