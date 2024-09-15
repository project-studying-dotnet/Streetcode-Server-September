using FluentValidation;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;

public class CreateStreetcodeCategoryValidator: AbstractValidator<CreateStreetcodeCategoryContentCommand>
{
    public CreateStreetcodeCategoryValidator(IRepositoryWrapper repositoryWrapper)
    {
        RuleFor(x => x.CategoryContentCreateDto.Text)
            .NotEmpty()
            .WithMessage("Category content text must not be empty")
            .MaximumLength(4000)
            .WithMessage("Category content text must not exceed 4000 symbols");

        RuleFor(x => x.CategoryContentCreateDto.StreetcodeId)
            .Must((command, _) =>
            {
                return repositoryWrapper.StreetcodeRepository
                    .GetFirstOrDefaultAsync(
                        streetcode => streetcode.Id == command.CategoryContentCreateDto.StreetcodeId)
                    .Result != null;
            })
            .WithMessage("Streetcode doesn't exist");
        
        RuleFor(x => x.CategoryContentCreateDto.SourceLinkCategoryId)
            .Must((command, _) =>
            {
                return repositoryWrapper.SourceCategoryRepository
                    .GetFirstOrDefaultAsync(
                        category => category.Id == command.CategoryContentCreateDto.SourceLinkCategoryId)
                    .Result != null;
            })
            .WithMessage("SourceLinkCategory doesn't exist");
    }
}