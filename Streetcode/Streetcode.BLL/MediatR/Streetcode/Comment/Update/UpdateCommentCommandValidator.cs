using FluentValidation;

namespace Streetcode.BLL.MediatR.Streetcode.Comment.Update
{
    public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
    {
        public UpdateCommentCommandValidator()
        {
            RuleFor(x => x.Comment.CommentContent)
                .NotEmpty()
                .WithMessage("Comment content is required")
                .MaximumLength(1000)
                .WithMessage("Comment content can not exceed 1000 characters");
        }
    }
}
