using FluentValidation;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;

public class CreateStreetcodeMainBlockCommandValidator: AbstractValidator<CreateStreetcodeMainBlockCommand>
{
    public CreateStreetcodeMainBlockCommandValidator()
    {
        RuleFor(x => x.StreetcodeMainBlockCreateDto.Index)
            .GreaterThanOrEqualTo(0).WithMessage("Index must be greater than or equal to 0.");
        
        RuleFor(x => x.StreetcodeMainBlockCreateDto.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.StreetcodeMainBlockCreateDto.FirstName)
            .NotNull()
            .When(x => x.StreetcodeMainBlockCreateDto.StreetcodeType == StreetcodeType.Person)
            .WithMessage("Person FirstName can not be null")
            .MaximumLength(50).WithMessage("FirstName must not exceed 50 characters.");

        RuleFor(x => x.StreetcodeMainBlockCreateDto.Rank)
            .MaximumLength(50).WithMessage("Rank must not exceed 50 characters.");

        RuleFor(x => x.StreetcodeMainBlockCreateDto.LastName)
            .NotNull()
            .When(x => x.StreetcodeMainBlockCreateDto.StreetcodeType == StreetcodeType.Person)
            .WithMessage("Person LastName can not be null")
            .MaximumLength(50).WithMessage("LastName must not exceed 50 characters.");

        RuleFor(x => x.StreetcodeMainBlockCreateDto.EventStartOrPersonBirthDate)
            .NotEmpty().WithMessage("EventStartOrPersonBirthDate is required.");

        RuleFor(x => x.StreetcodeMainBlockCreateDto.EventEndOrPersonDeathDate)
            .GreaterThan(x => x.StreetcodeMainBlockCreateDto.EventStartOrPersonBirthDate)
            .When(x => x.StreetcodeMainBlockCreateDto.EventEndOrPersonDeathDate.HasValue)
            .WithMessage("EventEndOrPersonDeathDate must be later than EventStartOrPersonBirthDate.");
        
        RuleFor(x => x.StreetcodeMainBlockCreateDto.Teaser)
            .MaximumLength(450).WithMessage("Teaser must not exceed 450 characters.");

        RuleFor(x => x.StreetcodeMainBlockCreateDto.TransliterationUrl)
            .NotEmpty().WithMessage("TransliterationUrl is required.")
            .MaximumLength(100).WithMessage("TransliterationUrl must not exceed 100 characters.")
            .Matches("^[a-z0-9-]+$")
            .WithMessage("TransliterationUrl can only contain lowercase Latin letters, numbers, and hyphens.");

        RuleFor(x => x.StreetcodeMainBlockCreateDto.BriefDescription)
            .MaximumLength(33).WithMessage("BriefDescription must not exceed 33 characters.");
        
        RuleFor(x => x.StreetcodeMainBlockCreateDto.BlackAndWhiteImageFileBaseCreateDto)
            .NotNull().WithMessage("BlackAndWhiteImageFileBaseCreateDto is required.");
    }
}