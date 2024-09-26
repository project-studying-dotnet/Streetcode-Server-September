using Moq;
using Xunit;
using MediatR;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Comment.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using commentEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Comment;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Comment.Delete;

public class DeleteCommentHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly DeleteCommentHandler _handler;

    public DeleteCommentHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _handler = new DeleteCommentHandler(_repositoryWrapperMock.Object);
    }

    [Fact]
    public async Task Handle_CommentExists_DeletesCommentAndReturnsSuccess()
    {
        // Arrange
        int commentId = 1;
        var comment = new commentEntity { Id = commentId };
        var command = new DeleteCommentCommand(commentId);

        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<commentEntity, bool>>>(),
            It.IsAny<Func<IQueryable<commentEntity>, IIncludableQueryable<commentEntity, object>>>()))
            .ReturnsAsync(comment);

        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.Delete(comment));

        _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<Unit>(result.Value);

        _repositoryWrapperMock.Verify(repo => repo.CommentRepository.Delete(comment), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CommentDoesNotExist_ThrowsCustomException()
    {
        // Arrange
        int commentId = 1;
        var command = new DeleteCommentCommand(commentId);

        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<commentEntity, bool>>>(),
            It.IsAny<Func<IQueryable<commentEntity>, IIncludableQueryable<commentEntity, object>>>()))
            .ReturnsAsync((commentEntity)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal($"No comment found by entered Id - {commentId}", exception.Message);
        Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);

        _repositoryWrapperMock.Verify(repo => repo.CommentRepository.Delete(It.IsAny<commentEntity>()), Times.Never);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_SaveChangesFails_ThrowsCustomException()
    {
        // Arrange
        int commentId = 1;
        var comment = new commentEntity { Id = commentId };
        var command = new DeleteCommentCommand(commentId);

        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<commentEntity, bool>>>(),
            It.IsAny<Func<IQueryable<commentEntity>, IIncludableQueryable<commentEntity, object>>>()))
            .ReturnsAsync(comment);

        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.Delete(comment));

        _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Failed to delete comment", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);

        _repositoryWrapperMock.Verify(repo => repo.CommentRepository.Delete(comment), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}
