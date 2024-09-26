using AutoMapper;
using FluentAssertions;
using Streetcode.BLL.Dto.Media.Video;
using Streetcode.BLL.Mapping.Media;
using Xunit;

namespace Streetcode.XUnitTest.MappingTests.Media.Video;

using Video = DAL.Entities.Media.Video;

public class VideoProfileTests
{
    private readonly IMapper _mapper;

    public VideoProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<VideoProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Should_Map_Video_To_VideoDto()
    {
        // Arrange
        var video = new Video
        {
            Id = 1,
            Title = "Test Video",
            Url = "https://www.youtube.com/watch?v=abc123"
        };

        // Act
        var videoDto = _mapper.Map<VideoDto>(video);

        // Assert
        videoDto.Should().NotBeNull();
        videoDto.Id.Should().Be(video.Id);
        videoDto.Title.Should().Be(video.Title);
        videoDto.Url.Should().Be(video.Url);
    }

    [Fact]
    public void Should_Map_VideoDto_To_Video()
    {
        // Arrange
        var videoDto = new VideoDto
        {
            Id = 1,
            Title = "Test Video",
            Url = "https://www.youtube.com/watch?v=abc123"
        };

        // Act
        var video = _mapper.Map<Video>(videoDto);

        // Assert
        video.Should().NotBeNull();
        video.Id.Should().Be(videoDto.Id);
        video.Title.Should().Be(videoDto.Title);
        video.Url.Should().Be(videoDto.Url);
    }

    [Fact]
    public void Should_Map_VideoCreateDto_To_Video()
    {
        // Arrange
        var videoCreateDto = new VideoCreateDto
        {
            Title = "New Video",
            Url = "https://www.youtube.com/watch?v=new123"
        };

        // Act
        var video = _mapper.Map<Video>(videoCreateDto);

        // Assert
        video.Should().NotBeNull();
        video.Title.Should().Be(videoCreateDto.Title);
        video.Url.Should().Be(videoCreateDto.Url);
    }
}
