using FluentValidation.TestHelper;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.MediatR.Streetcode.Text.Create;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Text;

public class CreateTextCommandValidatorTests
{
    private const int TitleMaxLength = 300;
    private const int TextContentMaxLength = 15000;
    private const int AdditionalTextMaxLength = 200;
    private readonly CreateTextCommandValidator _validator= new ();

    [Fact]
    public void Validator_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var invalidDto = new TextCreateDto
        {
            Title = "",
            TextContent = "null",
            AdditionalText = "null",
            StreetcodeId = 1
        };
        var invalidCommand = new CreateTextCommand(invalidDto);

        // Act
        var result = _validator.TestValidate(invalidCommand);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TextCreateDto.Title)
            .WithErrorMessage("Text title is required");
    }
    
    [Fact]
    public void Validator_ShouldFail_WhenTitleIsExceedsLengthLimit()
    {
        // Arrange
        var invalidDto = new TextCreateDto
        {
            Title = new string ('1', 301),
            TextContent = "null",
            AdditionalText = "null",
            StreetcodeId = 1
        };
        var invalidCommand = new CreateTextCommand(invalidDto);

        // Act
        var result = _validator.TestValidate(invalidCommand);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TextCreateDto.Title)
            .WithErrorMessage($"Text title must not exceed {TitleMaxLength} chars");
    }
    
    [Fact]
    public void Validator_ShouldFail_WhenTextContentIsEmpty()
    {
        // Arrange
        var invalidDto = new TextCreateDto
        {
            Title = "null",
            TextContent = "",
            AdditionalText = "null",
            StreetcodeId = 1
        };
        var invalidCommand = new CreateTextCommand(invalidDto);

        // Act
        var result = _validator.TestValidate(invalidCommand);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TextCreateDto.TextContent)
            .WithErrorMessage("Text content is required");
    }
    
    [Fact]
    public void Validator_ShouldFail_WhenTextContentIsExceedsLengthLimit()
    {
        // Arrange
        var invalidDto = new TextCreateDto
        {
            Title = "null",
            TextContent = new string('1', 15001),
            AdditionalText = "null",
            StreetcodeId = 1
        };
        var invalidCommand = new CreateTextCommand(invalidDto);

        // Act
        var result = _validator.TestValidate(invalidCommand);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TextCreateDto.TextContent)
            .WithErrorMessage($"Text content must not exceed {TextContentMaxLength} chars");
    }
    
    [Fact]
    public void Validator_ShouldFail_WhenAdditionalTextIsEmpty()
    {
        // Arrange
        var invalidDto = new TextCreateDto
        {
            Title = "null",
            TextContent = "null",
            AdditionalText = "",
            StreetcodeId = 1
        };
        var invalidCommand = new CreateTextCommand(invalidDto);

        // Act
        var result = _validator.TestValidate(invalidCommand);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TextCreateDto.AdditionalText)
            .WithErrorMessage("Text additional text is required");
    }
    
    [Fact]
    public void Validator_ShouldFail_WhenAdditionalTextIsExceedsLengthLimit()
    {
        // Arrange
        var invalidDto = new TextCreateDto
        {
            Title = "null",
            TextContent = "null",
            AdditionalText = new string('1', 201),
            StreetcodeId = 1
        };
        var invalidCommand = new CreateTextCommand(invalidDto);

        // Act
        var result = _validator.TestValidate(invalidCommand);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TextCreateDto.AdditionalText)
            .WithErrorMessage($"Text additional text must not exceed {AdditionalTextMaxLength} chars");
    }
    
    [Fact]
    public void Validator_ShouldFail_WhenStreetcodeIdIsZeroOrNegative()
    {
        // Arrange
        var invalidDto = new TextCreateDto
        {
            Title = "null",
            TextContent = "null",
            AdditionalText = "null",
            StreetcodeId = 0
        };
        var invalidCommand = new CreateTextCommand(invalidDto);

        // Act
        var result = _validator.TestValidate(invalidCommand);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TextCreateDto.StreetcodeId)
            .WithErrorMessage("Text StreetcodeId must be > 0");
    }
    
    [Fact]
    public void Validator_ShouldPass_WhenStreetcodeIdIsZeroOrNegative()
    {
        // Arrange
        var invalidDto = new TextCreateDto
        {
            Title = "null",
            TextContent = "null",
            AdditionalText = "null",
            StreetcodeId = 1
        };
        var invalidCommand = new CreateTextCommand(invalidDto);

        // Act
        var result = _validator.TestValidate(invalidCommand);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}