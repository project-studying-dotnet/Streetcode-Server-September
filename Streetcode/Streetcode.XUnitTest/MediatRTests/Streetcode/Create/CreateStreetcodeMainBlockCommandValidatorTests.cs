using FluentValidation.TestHelper;
using Streetcode.BLL.Dto.AdditionalContent.Tag;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;
using Streetcode.DAL.Enums;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Create;

public class CreateStreetcodeMainBlockCommandValidatorTests
{
    private readonly CreateStreetcodeMainBlockCommandValidator _validator;

    public CreateStreetcodeMainBlockCommandValidatorTests()
    {
        _validator = new CreateStreetcodeMainBlockCommandValidator();
    }

    [Fact]
    public void Index_ShouldBeValid_WhenGreaterThanOrEqualToZero()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto { Index = 1 });

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Index);
    }

    [Fact]
    public void Index_ShouldBeInvalid_WhenLessThanZero()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto { Index = -1 });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Index)
            .WithErrorMessage("Index must be greater than or equal to 0.");
    }
    
    [Fact]
    public void Title_ShouldBeValid_WhenNotEmptyAndMaxLengthIsNotExceeded()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto { Title = "Valid Title" });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldNotHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Title);
    }
    
    [Fact]
    public void Title_ShouldBeInvalid_WhenEmpty()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto { Title = "" });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Title)
            .WithErrorMessage("Title is required.");
    }
    
    [Fact]
    public void Title_ShouldBeInvalid_WhenExceedsMaxLength()
    {
        var command = new CreateStreetcodeMainBlockCommand(
            new StreetcodeMainBlockCreateDto { Title = new string('a', 101) });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Title)
            .WithErrorMessage("Title must not exceed 100 characters.");
    }
    
    [Fact]
    public void FirstName_ShouldBeValid_WhenNotNullAndMaxLengthIsNotExceeded_WhenStreetcodeTypeIsPerson()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto
        {
            StreetcodeType = StreetcodeType.Person,
            FirstName = "John"
        });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldNotHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.FirstName);
    }
    
    [Fact]
    public void FirstName_ShouldBeInvalid_WhenNull_WhenStreetcodeTypeIsPerson()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto
        {
            StreetcodeType = StreetcodeType.Person,
            FirstName = null
        });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.FirstName)
            .WithErrorMessage("Person FirstName can not be null");
    }
    
    [Fact]
    public void Rank_ShouldBeValid_WhenMaxLengthIsNotExceeded()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto { Rank = "Valid Rank" });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldNotHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Rank);
    }
    
    [Fact]
    public void Rank_ShouldBeInvalid_WhenExceedsMaxLength()
    {
        var command = new CreateStreetcodeMainBlockCommand(
            new StreetcodeMainBlockCreateDto { Rank = new string('a', 51) });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Rank)
            .WithErrorMessage("Rank must not exceed 50 characters.");
    }
    
    [Fact]
    public void LastName_ShouldBeValid_WhenNotNullAndMaxLengthIsNotExceeded_WhenStreetcodeTypeIsPerson()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto
        {
            StreetcodeType = StreetcodeType.Person,
            LastName = "Doe"
        });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldNotHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.LastName);
    }
    
    [Fact]
    public void LastName_ShouldBeInvalid_WhenNull_WhenStreetcodeTypeIsPerson()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto
        {
            StreetcodeType = StreetcodeType.Person,
            LastName = null
        });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.LastName)
            .WithErrorMessage("Person LastName can not be null");
    }
    
    [Fact]
    public void EventStartOrPersonBirthDate_ShouldBeValid_WhenNotEmpty()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto
        {
            EventStartOrPersonBirthDate = DateTime.Now
        });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldNotHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.EventStartOrPersonBirthDate);
    }
    
    [Fact]
    public void EventEndOrPersonDeathDate_ShouldBeInvalid_WhenNotGreaterThanEventStartOrPersonBirthDate()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto
        {
            EventStartOrPersonBirthDate = DateTime.Now,
            EventEndOrPersonDeathDate = DateTime.Now.AddDays(-1)
        });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.EventEndOrPersonDeathDate)
            .WithErrorMessage("EventEndOrPersonDeathDate must be later than EventStartOrPersonBirthDate.");
    }
    
    [Fact]
    public void Teaser_ShouldBeValid_WhenMaxLengthIsNotExceeded()
    {
        var command = new CreateStreetcodeMainBlockCommand(
            new StreetcodeMainBlockCreateDto { Teaser = "Short teaser" });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldNotHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Teaser);
    }
    
    [Fact]
    public void Teaser_ShouldBeInvalid_WhenExceedsMaxLength()
    {
        var command = new CreateStreetcodeMainBlockCommand(
            new StreetcodeMainBlockCreateDto { Teaser = new string('a', 451) });
        
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Teaser)
            .WithErrorMessage("Teaser must not exceed 450 characters.");
    }
    
    [Fact]
    public void TransliterationUrl_ShouldBeValid_WhenNotEmptyAndMatchesPattern()
    {
        var command = new CreateStreetcodeMainBlockCommand(
            new StreetcodeMainBlockCreateDto { TransliterationUrl = "valid-url" });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldNotHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.TransliterationUrl);
    }
    
    [Fact]
    public void TransliterationUrl_ShouldBeInvalid_WhenEmpty()
    {
        var command = new CreateStreetcodeMainBlockCommand(
            new StreetcodeMainBlockCreateDto { TransliterationUrl = "" });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.TransliterationUrl)
            .WithErrorMessage("TransliterationUrl is required.");
    }
    
    [Fact]
    public void TransliterationUrl_ShouldBeInvalid_WhenExceedsMaxLength()
    {
        var command = new CreateStreetcodeMainBlockCommand(
            new StreetcodeMainBlockCreateDto { TransliterationUrl = new string('a', 101) });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.TransliterationUrl)
            .WithErrorMessage("TransliterationUrl must not exceed 100 characters.");
    }
    
    [Fact]
    public void Tags_ShouldBeValid_WhenNotEmpty()
    {
        var command = new CreateStreetcodeMainBlockCommand(new StreetcodeMainBlockCreateDto
        {
            Tags = new List<TagShortDto> { new (), new () }
        });
    
        var result = _validator.TestValidate(command);
    
        result.ShouldNotHaveValidationErrorFor(x => x.StreetcodeMainBlockCreateDto.Tags);
    }
}