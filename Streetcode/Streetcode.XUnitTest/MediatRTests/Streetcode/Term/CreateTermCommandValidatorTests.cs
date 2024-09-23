using FluentValidation.TestHelper;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;
using Streetcode.BLL.MediatR.Streetcode.Term.Create;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Term;

public class CreateTermCommandValidatorTests
{
    private readonly CreateTermCommandValidator _validator = new();

    [Fact]
    public void Validator_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var invalidDto = new TermCreateDto(string.Empty, "Description");
        var invalidCommand = new CreateTermCommand(invalidDto);
        
        // Act
        var result = _validator.TestValidate(invalidCommand);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TermCreateDto.Title).WithErrorMessage("Term title is required");
    }
    
    [Fact]
    public void Validator_ShouldFail_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var invalidDto = new TermCreateDto(new string('a', 51), "Description");
        var invalidCommand = new CreateTermCommand(invalidDto);
        
        // Act
        var result = _validator.TestValidate(invalidCommand);
        
        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TermCreateDto.Title)
            .WithErrorMessage("Title should not exceed 50 chars");
    }
    
    [Fact]
    public void Validator_ShouldFail_WhenDescriptionIsEmpty()
    {
        // Arrange
        var invalidDto = new TermCreateDto("Title", string.Empty);
        var invalidCommand = new CreateTermCommand(invalidDto);
        
        // Act
        var result = _validator.TestValidate(invalidCommand);
        
        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TermCreateDto.Description)
            .WithErrorMessage("Term description is required");
    }
    
    [Fact]
    public void Validator_ShouldFail_WhenDescriptionExceedsMaxLength()
    {
        // Arrange
        var invalidDto = new TermCreateDto("title", new string('a', 501));
        var invalidCommand = new CreateTermCommand(invalidDto);
        
        // Act
        var result = _validator.TestValidate(invalidCommand);
        
        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TermCreateDto.Description)
            .WithErrorMessage("Description should not exceed 500 chars");
    }
    
    [Fact]
    public void Validator_ShouldPass_ForValidInput()
    {
        // Arrange
        var validDto = new TermCreateDto("Title", "Description");
        var validCommand = new CreateTermCommand(validDto);
        
        // Act
        var result = _validator.TestValidate(validCommand);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}