using FluentValidation;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Update
{
    public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
    {
        public UpdateCommentCommandValidator(IRepositoryWrapper repositoryWrapper)
        {
            RuleFor(x => x.Comment.CommentContent)
                .NotEmpty()
                .WithMessage("Comment content is required")
                .MaximumLength(1000)
                .WithMessage("Comment content can not exceed 1000 characters");
        }
    }
}
