using FluentValidation;
using Streetcode.DAL.Repositories.Interfaces.Base;


namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;

public class UpdateStreetcodeCategoryValidator : AbstractValidator<UpdateStreetcodeCategoryContentCommand>
{
    public UpdateStreetcodeCategoryValidator(IRepositoryWrapper repository)
    {
        RuleFor(x => x.CategoryContentUpdateDto.Text)
            .NotEmpty()
            .WithMessage("Category content text must not be empty")
            .MaximumLength(4000)
            .WithMessage("Category content text must not exceed 4000 symbols");


        RuleFor(x => x.CategoryContentUpdateDto.SourceLinkCategoryId)
        .Must((command, _) =>
        {
                return repository.SourceCategoryRepository
                    .GetFirstOrDefaultAsync(
                        category => category.Id == command.CategoryContentUpdateDto.SourceLinkCategoryId)
                    .Result != null;
            })
            .WithMessage("SourceLinkCategory doesn't exist");
    }
}
