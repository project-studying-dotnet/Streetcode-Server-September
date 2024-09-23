using FluentValidation;

namespace Streetcode.BLL.MediatR.Media.Art.Create;

public class CreateArtCommandValidator: AbstractValidator<CreateArtCommand>
{
    public CreateArtCommandValidator()
    {
        RuleFor(x => x.newArt.Title)
              .NotEmpty().WithMessage("Title is required.")
              .MaximumLength(150).WithMessage("Title must be at most 150 characters long.");

        RuleFor(x => x.newArt.Description)
            .MaximumLength(400).WithMessage("Description must be at most 400 characters long.");

        RuleFor(x => x.newArt.ImageId)
            .GreaterThan(0)
            .WithMessage("ImageId must be a positive integer."); ;

        RuleFor(x => x.newArt.Streetcodes)
            .NotNull()
            .WithMessage("Streetcodes list cannot be null.")
            .Must(streetcodes => streetcodes.Count > 0)
            .WithMessage("At least one streetcode is required.");
    }
}
