using FluentValidation;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Create;

public class CreateTermCommandValidator: AbstractValidator<CreateTermCommand>
{
    public CreateTermCommandValidator()
    {
        RuleFor(x => x.TermCreateDto.Title)
            .NotEmpty().WithMessage("Term title is required")
            .MaximumLength(50).WithMessage("Title should not exceed 50 chars");
        
        RuleFor(x => x.TermCreateDto.Description)
            .NotEmpty().WithMessage("Term description is required")
            .MaximumLength(500).WithMessage("Description should not exceed 500 chars");
    }
}