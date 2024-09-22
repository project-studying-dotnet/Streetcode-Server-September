using FluentValidation.TestHelper;
using Moq;
using Xunit;
using Streetcode.BLL.MediatR.Streetcode.Comment.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.DAL.Entities.Streetcode;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Comment.Create
{
    public class CreateCommentCommandValidatorTests
    {
        private readonly CreateCommentCommandValidator _validator;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;

        public CreateCommentCommandValidatorTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();

            // Настраиваем возвращаемое значение для StreetcodeRepository
            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(new StreetcodeContent { Id = 1 });

            _validator = new CreateCommentCommandValidator(_repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            // Arrange
            var command = new CreateCommentCommand(new CommentCreateDto
            {
                CommentContent = "Valid comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = DateTime.Now
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
            var command = new CreateCommentCommand(new CommentCreateDto
            {
                CommentContent = "",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = DateTime.Now
            });

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Comment.CommentContent)
                .WithErrorMessage("Comment Content is required");
        }

        [Fact]
        public async Task Validate_CommentContentExceedsMaxLength_FailsValidation()
        {
            // Arrange
            var longContent = new string('a', 1001); // Длина 1001 символ
            var command = new CreateCommentCommand(new CommentCreateDto
            {
                CommentContent = longContent,
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = DateTime.Now
            });

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Comment.CommentContent)
                .WithErrorMessage("Comment Content can not exceed 1000 characters");
        }

        [Fact]
        public async Task Validate_StreetcodeIdDoesNotExist_FailsValidation()
        {
            // Arrange
            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync((StreetcodeContent)null); // Симулируем отсутствие Streetcode

            var command = new CreateCommentCommand(new CommentCreateDto
            {
                CommentContent = "Valid comment",
                StreetcodeId = 999, // Несуществующий ID
                UserId = 1,
                DateCreated = DateTime.Now
            });

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Comment.StreetcodeId)
                .WithErrorMessage("Streetcode Id doesn't exist");
        }

        [Fact]
        public async Task Validate_DateCreatedIsEmpty_FailsValidation()
        {
            // Arrange
            var command = new CreateCommentCommand(new CommentCreateDto
            {
                CommentContent = "Valid comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = default
            });

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Comment.DateCreated)
                .WithErrorMessage("Date of creation commet is required");
        }
    }
}
