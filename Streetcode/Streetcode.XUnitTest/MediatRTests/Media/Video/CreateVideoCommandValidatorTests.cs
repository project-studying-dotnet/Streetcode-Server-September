using FluentValidation.TestHelper;
using Streetcode.BLL.Dto.Media.Video;
using Streetcode.BLL.MediatR.Media.Video.Create;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Media.Video;

public class CreateVideoCommandValidatorTests
{
    private readonly CreateVideoCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenTitleAndUrlAreValid()
    {
        // Arrange
        var validCommand = new CreateVideoCommand(new VideoCreateDto
        {
            Title = "Valid Title",
            Url = "https://www.youtube.com/watch?v=abc123"
        });

        // Act
        var result = _validator.TestValidate(validCommand);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var longTitle = new string('a', 101); // 101 characters long
        var command = new CreateVideoCommand(new VideoCreateDto
        {
            Title = longTitle,
            Url = "https://www.youtube.com/watch?v=abc123"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VideoCreateDto.Title)
              .WithErrorMessage("Video Title should nor exceed 100 chars");
    }

    [Fact]
    public void Validate_ShouldFail_WhenUrlIsEmpty()
    {
        // Arrange
        var command = new CreateVideoCommand(new VideoCreateDto
        {
            Title = "Valid Title",
            Url = ""
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VideoCreateDto.Url)
              .WithErrorMessage("Youtube url is required");
    }

    [Fact]
    public void Validate_ShouldFail_WhenUrlIsNotYoutubeLink()
    {
        // Arrange
        var command = new CreateVideoCommand(new VideoCreateDto
        {
            Title = "Valid Title",
            Url = "https://www.vimeo.com/watch?v=abc123"
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VideoCreateDto.Url)
              .WithErrorMessage("Video url must be in youtube.com domain");
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=abc123")]
    [InlineData("https://youtu.be/abc123")]
    [InlineData("http://youtube.com/watch?v=abc123")]
    [InlineData("https://www.youtube.com/embed/abc123")]
    public void Validate_ShouldPass_WhenUrlIsValidYoutubeLink(string validYoutubeUrl)
    {
        // Arrange
        var command = new CreateVideoCommand(new VideoCreateDto
        {
            Title = "Valid Title",
            Url = validYoutubeUrl
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VideoCreateDto.Url);
    }
}
