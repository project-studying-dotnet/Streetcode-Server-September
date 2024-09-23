using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Create;

using FluentValidation;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator(IRepositoryWrapper repositoryWrapper)
    {
        RuleFor(x => x.Comment.CommentContent)
            .NotEmpty()
            .WithMessage("Comment Content is required")
            .MaximumLength(1000)
            .WithMessage("Comment Content can not exceed 1000 characters");

        RuleFor(x => x.Comment.StreetcodeId)
            .Must((StreetcodeId, _) =>
            {
                return repositoryWrapper.StreetcodeRepository
                    .GetFirstOrDefaultAsync(Streetcode => Streetcode.Id == StreetcodeId.Comment.StreetcodeId)
                    .Result != null;
            })
            .WithMessage("Streetcode Id doesn't exist");


        RuleFor(x => x.Comment.DateCreated)
            .NotEmpty()
            .WithMessage("Date of creation commet is required");

        /*RuleFor(x => x.Comment.UserId)
            .Must((userId, _) =>
            {
                return repositoryWrapper.UserRepository
                    .GetFirstOrDefaultAsync(user => user.Id == userId.Comment.UserId)
                    .Result != null;
            })
            .WithMessage("User Id doesn't exist");*/
    }
}
