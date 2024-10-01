using System.Linq.Expressions;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Comment.Approve;
using Streetcode.BLL.Resources;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Comment;

using Comment = DAL.Entities.Streetcode.TextContent.Comment;

public class ApproveCommentHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IStringLocalizer<ErrorMessages>> _mockLocalizer;
    private readonly ApproveCommentHandler _handler;

    public ApproveCommentHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockLocalizer = new Mock<IStringLocalizer<ErrorMessages>>();
        _handler = new ApproveCommentHandler(_mockRepositoryWrapper.Object, _mockLocalizer.Object);
    }

    [Fact]
    public async Task Handle_CommentExistsAndIsPending_ShouldApproveComment()
    {
        // Arrange
        var request = new ApproveCommentQuery(1);
        var comment = new Comment { Id = 1, Status = CommentStatus.Pending };
        
        _mockRepositoryWrapper
            .Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Comment, bool>>>(),
                null))
            .ReturnsAsync(comment);
        _mockRepositoryWrapper
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
        comment.Status.Should().Be(CommentStatus.Approved);
        _mockRepositoryWrapper.Verify(r => r.CommentRepository.Update(comment), Times.Once);
    }

    [Fact]
    public async Task Handle_CommentDoesNotExist_ShouldThrowNotFoundCustomException()
    {
        // Arrange
        var request = new ApproveCommentQuery(1);
        string errorMsg = $"No Comment with {request.Id} was found";
        
        _mockRepositoryWrapper
            .Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Comment, bool>>>(), 
                null))
            .ReturnsAsync((Comment)null!);

        _mockLocalizer
            .Setup(s => s[ErrorKeys.NotFoundError])
            .Returns(new LocalizedString(ErrorKeys.NotFoundError, errorMsg));

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<CustomException>()
            .WithMessage(errorMsg)
            .Where(e => e.StatusCode == StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Handle_CommentIsAlreadyApproved_ShouldThrowCustomException()
    {
        // Arrange
        var request = new ApproveCommentQuery(1);
        var comment = new Comment { Id = 1, Status = CommentStatus.Approved };

        _mockRepositoryWrapper
            .Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Comment, bool>>>(), 
                null))
            .ReturnsAsync(comment);

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<CustomException>()
            .WithMessage("Comment can not be approved twice!")
            .Where(e => e.StatusCode == StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ShouldThrowCustomException()
    {
        // Arrange
        var request = new ApproveCommentQuery(1);
        var comment = new Comment { Id = 1, Status = CommentStatus.Pending };

        _mockRepositoryWrapper
            .Setup(r => r.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Comment, bool>>>(), 
                null))
            .ReturnsAsync(comment);
        _mockRepositoryWrapper
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(0);

        _mockLocalizer
            .Setup(s => s[ErrorKeys.SaveChangesError])
            .Returns(new LocalizedString(ErrorKeys.SaveChangesError, "Save Changes failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<CustomException>()
            .WithMessage("Save Changes failed")
            .Where(e => e.StatusCode == StatusCodes.Status500InternalServerError);
    }
}
