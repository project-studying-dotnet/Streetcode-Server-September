using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create;

using FluentValidation;
using Repositories.Interfaces;

public class CreateSourceLinkCategoryCommandValidator: AbstractValidator<CreateSourceLinkCategoryCommand>
{
    public CreateSourceLinkCategoryCommandValidator(IRepositoryWrapper repositoryWrapper)
    {
        RuleFor(x => x.SrcLinkCategoryCreateDto.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(100)
            .WithMessage("Title can not exceed 100 characters");

        RuleFor(x => x.SrcLinkCategoryCreateDto.ImageId)
            .Must((imageId, _) =>
            {
                return repositoryWrapper.ImageRepository
                    .GetFirstOrDefaultAsync(image => image.Id == imageId.SrcLinkCategoryCreateDto.ImageId)
                    .Result != null;
            })
            .WithMessage("Image Id doesn't exist");
    }
}