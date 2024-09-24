using FluentValidation.TestHelper;
using Moq;
using Xunit;
using Streetcode.BLL.MediatR.Streetcode.Comment.Update;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Comment.Update
{
    public class UpdateCommentCommandValidatorTests
    {
        private readonly UpdateCommentCommandValidator _validator;

        public UpdateCommentCommandValidatorTests()
        {
            _validator = new UpdateCommentCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            // Arrange
            var command = new UpdateCommentCommand(new CommentUpdateDto
            {
                Id = 1,
                CommentContent = "This is a valid comment."
            });

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyCommentContent_FailsValidation()
        {
            // Arrange
            var command = new UpdateCommentCommand(new CommentUpdateDto
            {
                Id = 1,
                CommentContent = ""
            });

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Comment.CommentContent)
                .WithErrorMessage("Comment content is required");
        }

        [Fact]
        public async Task Validate_CommentContentExceedsMaxLength_FailsValidation()
        {
            // Arrange
            var longContent = new string('a', 1001);
            var command = new UpdateCommentCommand(new CommentUpdateDto
            {
                Id = 1,
                CommentContent = longContent
            });

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Comment.CommentContent)
                .WithErrorMessage("Comment content can not exceed 1000 characters");
        }
    }
}
