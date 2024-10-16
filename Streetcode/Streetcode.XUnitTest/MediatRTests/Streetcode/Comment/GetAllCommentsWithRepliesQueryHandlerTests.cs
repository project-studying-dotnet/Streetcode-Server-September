using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetAllCommentsWithReplies;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Comment
{
    public class GetAllCommentsWithRepliesQueryHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetAllCommentsWithRepliesQueryHandler _handler;

        public GetAllCommentsWithRepliesQueryHandlerTests()
        {
            _mockRepo = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetAllCommentsWithRepliesQueryHandler(_mockRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenCommentsExist()
        {
            // Arrange
            var comments = new List<DAL.Entities.Streetcode.TextContent.Comment>
        {
            new DAL.Entities.Streetcode.TextContent.Comment
            {
                Id = 1,
                CommentContent = "Parent Comment",
                ParentCommentId = null
            },
            new DAL.Entities.Streetcode.TextContent.Comment
            {
                Id = 2,
                CommentContent = "Reply 1",
                ParentCommentId = 1
            },
            new DAL.Entities.Streetcode.TextContent.Comment
            {
                Id = 3,
                CommentContent = "Reply 2",
                ParentCommentId = 1
            },
            new DAL.Entities.Streetcode.TextContent.Comment
            {
                Id = 4,
                CommentContent = "Reply to Reply 1",
                ParentCommentId = 2
            }
        };

            _mockRepo.Setup(repo => repo.CommentRepository.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.Comment, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.Comment>, IIncludableQueryable<DAL.Entities.Streetcode.TextContent.Comment, object>>>())).ReturnsAsync(comments);

            _mockMapper.Setup(mapper => mapper.Map<List<CommentWithRepliesDto>>(It.IsAny<List<DAL.Entities.Streetcode.TextContent.Comment>>()))
                .Returns((List<DAL.Entities.Streetcode.TextContent.Comment> sourceComments) =>
                {
                    return sourceComments.Select(c => new CommentWithRepliesDto
                    {
                        Id = c.Id,
                        CommentContent = c.CommentContent,
                        ParentCommentId = c.ParentCommentId,
                        Replies = new List<CommentWithRepliesDto>()
                    }).ToList();
                });

            // Act
            var result = await _handler.Handle(new GetAllCommentsWithRepliesQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            var topLevelComment = result.Value[0];
            Assert.Equal(1, topLevelComment.Id);
            Assert.Equal(2, topLevelComment.Replies.Count);
            var reply1 = topLevelComment.Replies.FirstOrDefault(r => r.Id == 2);
            Assert.NotNull(reply1);
            Assert.Single(reply1.Replies);
            Assert.Equal(4, reply1.Replies[0].Id);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenExceptionThrown()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.CommentRepository.GetAllAsync(null, null))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(new GetAllCommentsWithRepliesQuery(), CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Database error", result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoCommentsExist()
        {
            // Arrange
            var emptyCommentList = new List<DAL.Entities.Streetcode.TextContent.Comment>();

            _mockRepo.Setup(repo => repo.CommentRepository.GetAllAsync(It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.Comment, bool>>>(), It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.Comment>, IIncludableQueryable<DAL.Entities.Streetcode.TextContent.Comment, object>>>()))
                .ReturnsAsync(emptyCommentList);

            _mockMapper.Setup(mapper => mapper.Map<List<CommentWithRepliesDto>>(emptyCommentList))
                .Returns(new List<CommentWithRepliesDto>());

            // Act
            var result = await _handler.Handle(new GetAllCommentsWithRepliesQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }
    }
}
