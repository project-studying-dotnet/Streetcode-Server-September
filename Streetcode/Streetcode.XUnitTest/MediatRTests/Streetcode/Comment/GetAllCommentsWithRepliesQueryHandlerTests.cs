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
                Replies = new List<DAL.Entities.Streetcode.TextContent.Comment>
                {
                 new DAL.Entities.Streetcode.TextContent.Comment { Id = 2, CommentContent = "Reply 1" },
                 new DAL.Entities.Streetcode.TextContent.Comment { Id = 3, CommentContent = "Reply 2" }
                }
              }
            };

            _mockRepo.Setup(repo => repo.CommentRepository.GetAllAsync(
             It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.Comment, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.Comment>, IIncludableQueryable<DAL.Entities.Streetcode.TextContent.Comment, object>>>())).ReturnsAsync(comments);

            _mockMapper.Setup(mapper => mapper.Map<List<CommentWithRepliesDto>>(comments))
                .Returns(new List<CommentWithRepliesDto>
                {
                    new CommentWithRepliesDto
                    {
                        Id = 1,
                        CommentContent = "Parent Comment",
                        Replies = new List<CommentDto>
                        {
                            new CommentDto { Id = 2, CommentContent = "Reply 1" },
                            new CommentDto { Id = 3, CommentContent = "Reply 2" }
                        }
                    }
                });

            // Act
            var result = await _handler.Handle(new GetAllCommentsWithRepliesQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.Equal(2, result.Value[0].Replies.Count);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenExceptionThrown()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.CommentRepository.GetAllAsync(
            It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.Comment, bool>>>(),
            It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.Comment>, IIncludableQueryable<DAL.Entities.Streetcode.TextContent.Comment, object>>>())).ThrowsAsync(new Exception("Database error"));

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

            _mockRepo.Setup(repo => repo.CommentRepository.GetAllAsync(
              It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.Comment, bool>>>(),
             It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.Comment>, IIncludableQueryable<DAL.Entities.Streetcode.TextContent.Comment, object>>>())).ReturnsAsync(emptyCommentList); // Use emptyCommentList instead of comments

            _mockMapper.Setup(mapper => mapper.Map<List<CommentWithRepliesDto>>(emptyCommentList))
                .Returns(new List<CommentWithRepliesDto>());

            // Act
            var result = await _handler.Handle(new GetAllCommentsWithRepliesQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task Handle_ReturnsNull_WhenMappingReturnsNull()
        {
            // Arrange
            var comments = new List<DAL.Entities.Streetcode.TextContent.Comment>
            {
                new DAL.Entities.Streetcode.TextContent.Comment
                {
                    Id = 1,
                    CommentContent = "Parent Comment",
                    Replies = new List<DAL.Entities.Streetcode.TextContent.Comment>
                    {
                        new DAL.Entities.Streetcode.TextContent.Comment { Id = 2, CommentContent = "Reply 1" },
                        new DAL.Entities.Streetcode.TextContent.Comment { Id = 3, CommentContent = "Reply 2" }
                    }
                }
            };

            _mockRepo.Setup(repo => repo.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.Comment, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.Comment>, IIncludableQueryable<DAL.Entities.Streetcode.TextContent.Comment, object>>>())).ReturnsAsync(comments);

            _mockMapper.Setup(mapper => mapper.Map<List<CommentWithRepliesDto>>(comments))
                .Returns((List<CommentWithRepliesDto>)null);

            // Act
            var result = await _handler.Handle(new GetAllCommentsWithRepliesQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public async Task Handle_ExecutesRepositoryAndMapper_Correctly()
        {
            // Arrange
            var comments = new List<DAL.Entities.Streetcode.TextContent.Comment>
            {
                new DAL.Entities.Streetcode.TextContent.Comment
                {
                    Id = 1,
                    CommentContent = "Parent Comment",
                    Replies = new List<DAL.Entities.Streetcode.TextContent.Comment>
                    {
                        new DAL.Entities.Streetcode.TextContent.Comment { Id = 2, CommentContent = "Reply 1" },
                        new DAL.Entities.Streetcode.TextContent.Comment { Id = 3, CommentContent = "Reply 2" }
                    }
                }
            };

            _mockRepo.Setup(repo => repo.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.Comment, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.Comment>, IIncludableQueryable<DAL.Entities.Streetcode.TextContent.Comment, object>>>())).ReturnsAsync(comments);

            _mockMapper.Setup(mapper => mapper.Map<List<CommentWithRepliesDto>>(comments))
                .Returns(new List<CommentWithRepliesDto>
                {
                    new CommentWithRepliesDto
                    {
                        Id = 1,
                        CommentContent = "Parent Comment",
                        Replies = new List<CommentDto>
                        {
                            new CommentDto { Id = 2, CommentContent = "Reply 1" },
                            new CommentDto { Id = 3, CommentContent = "Reply 2" }
                        }
                    }
                });

            // Act
            var result = await _handler.Handle(new GetAllCommentsWithRepliesQuery(), CancellationToken.None);

            // Assert
            _mockRepo.Verify(repo => repo.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<DAL.Entities.Streetcode.TextContent.Comment, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Streetcode.TextContent.Comment>, IIncludableQueryable<DAL.Entities.Streetcode.TextContent.Comment, object>>>()), Times.Once);

            _mockMapper.Verify(mapper => mapper.Map<List<CommentWithRepliesDto>>(comments), Times.Once);
        }
    }
}
