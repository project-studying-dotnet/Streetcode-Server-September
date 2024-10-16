using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create;

using FluentValidation;

public class CreateFactCommandValidator : AbstractValidator<CreateFactCommand>
{
    public CreateFactCommandValidator(IRepositoryWrapper repositoryWrapper)
    {
        RuleFor(x => x.Fact.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(68)
            .WithMessage("Title can not exceed 68 characters");

        RuleFor(x => x.Fact.ImageId)
            .Must((imageId, _) =>
            {
                return repositoryWrapper.ImageRepository
                    .GetFirstOrDefaultAsync(image => image.Id == imageId.Fact.ImageId)
                    .Result != null;
            })
            .WithMessage("Image Id doesn't exist");

        RuleFor(x => x.Fact.FactContent)
            .NotEmpty()
            .WithMessage("Main text is required")
            .MaximumLength(600)
            .WithMessage("Main text can not exceed 600 characters");

        RuleFor(x => x.Fact.StreetcodeId)
            .Must((StreetcodeId, _) =>
            {
                return repositoryWrapper.StreetcodeRepository
                    .GetFirstOrDefaultAsync(Streetcode => Streetcode.Id == StreetcodeId.Fact.StreetcodeId)
                    .Result != null;
            })
            .WithMessage("Streetcode Id doesn't exist");
    }
}
