using AutoMapper;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetRepliesByCommentId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using CommentEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Comment;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Comment;


public class GetRepliesByCommentIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetRepliesByCommentIdHandler _handler;

    public GetRepliesByCommentIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetRepliesByCommentIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_RepliesFound_ReturnsSuccessResult()
    {
        // Arrange
        int commentId = 1;
        var replies = new List<CommentEntity>
        {
            new CommentEntity { Id = 2, CommentContent = "Reply 1", ParentCommentId = commentId },
            new CommentEntity { Id = 3, CommentContent = "Reply 2", ParentCommentId = commentId }
        };
        var replyDtos = new List<CommentDto>
        {
            new CommentDto { Id = 2, CommentContent = "Reply 1", ParentCommentId = commentId },
            new CommentDto { Id = 3, CommentContent = "Reply 2", ParentCommentId = commentId }
        };
        var query = new GetRepliesByCommentIdQuery(commentId);

        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetAllAsync(
            It.IsAny<Expression<Func<CommentEntity, bool>>>(),
            It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()))
            .ReturnsAsync(replies);

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<CommentDto>>(replies))
            .Returns(replyDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(replyDtos, result.Value);

        _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetAllAsync(
            It.Is<Expression<Func<CommentEntity, bool>>>(expr => TestExpression(expr, commentId)),
            It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Once);

        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<CommentDto>>(replies), Times.Once);
    }

    [Fact]
    public async Task Handle_RepliesNotFound_ReturnsEmptyList()
    {
        // Arrange
        int commentId = 1;
        var replies = new List<CommentEntity>(); 
        var replyDtos = new List<CommentDto>();
        var query = new GetRepliesByCommentIdQuery(commentId);

        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetAllAsync(
            It.IsAny<Expression<Func<CommentEntity, bool>>>(),
            It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()))
            .ReturnsAsync(replies);

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<CommentDto>>(replies))
            .Returns(replyDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);

        _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetAllAsync(
            It.Is<Expression<Func<CommentEntity, bool>>>(expr => TestExpression(expr, commentId)),
            It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Once);

        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<CommentDto>>(replies), Times.Once);
    }

    [Fact]
    public async Task Handle_GetAllAsyncReturnsNull_ThrowsCustomException()
    {
        // Arrange
        int commentId = 1;
        var query = new GetRepliesByCommentIdQuery(commentId);

        _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetAllAsync(
            It.IsAny<Expression<Func<CommentEntity, bool>>>(),
            It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()))
            .ReturnsAsync((IEnumerable<CommentEntity>)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(query, CancellationToken.None));

        Assert.Equal($"Cannot find any replies by the comment id: {commentId}", exception.Message);
        Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);

        _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetAllAsync(
            It.Is<Expression<Func<CommentEntity, bool>>>(expr => TestExpression(expr, commentId)),
            It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Once);

        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<CommentDto>>(It.IsAny<IEnumerable<CommentEntity>>()), Times.Never);
    }

    // Helper method to test the expression used in GetAllAsync
    private bool TestExpression(Expression<Func<CommentEntity, bool>> expression, int expectedCommentId)
    {
        // Compile the expression into a function
        var func = expression.Compile();

        // Create a test CommentEntity with matching ParentCommentId
        var testEntity = new CommentEntity { ParentCommentId = expectedCommentId };

        // Check if the expression returns true for the test entity
        return func(testEntity);
    }
}
