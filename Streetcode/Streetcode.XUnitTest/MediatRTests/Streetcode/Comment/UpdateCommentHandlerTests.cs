using AutoMapper;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Comment.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using CommentEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Comment;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Comment.Update
{
    public class UpdateCommentHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateCommentHandler _handler;

        public UpdateCommentHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _handler = new UpdateCommentHandler(_repositoryWrapperMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_UpdatesCommentAndReturnsSuccess()
        {
            // Arrange
            var commentUpdateDto = new CommentUpdateDto
            {
                Id = 1,
                CommentContent = "Updated comment"
            };

            var commentEntityMapped = new CommentEntity
            {
                Id = 1,
                CommentContent = "Updated comment"
            };

            var existingCommentEntity = new CommentEntity
            {
                Id = 1,
                CommentContent = "Original comment",
                DateCreated = DateTime.Now.AddDays(-1),
                StreetcodeId = 1,
                UserId = 1
            };

            var updatedCommentEntity = new CommentEntity
            {
                Id = 1,
                CommentContent = "Updated comment",
                DateCreated = existingCommentEntity.DateCreated,
                StreetcodeId = existingCommentEntity.StreetcodeId,
                UserId = existingCommentEntity.UserId
            };

            var expectedResultDto = new CommentDto
            {
                Id = 1,
                CommentContent = "Updated comment",
                DateCreated = existingCommentEntity.DateCreated,
                StreetcodeId = existingCommentEntity.StreetcodeId,
                UserId = existingCommentEntity.UserId
            };

            var command = new UpdateCommentCommand(commentUpdateDto);

            // Setup mapping from CommentUpdateDto to CommentEntity
            _mapperMock.Setup(m => m.Map<CommentEntity>(commentUpdateDto))
                .Returns(commentEntityMapped);

            // Setup repository to return existing comment
            _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()))
                .ReturnsAsync(existingCommentEntity);

            // Setup mapping from CommentEntity to CommentDto
            _mapperMock.Setup(m => m.Map<CommentDto>(It.IsAny<CommentEntity>()))
                .Returns(expectedResultDto);

            // Setup SaveChangesAsync to return success
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResultDto, result.Value);

            _mapperMock.Verify(m => m.Map<CommentEntity>(commentUpdateDto), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.Update(It.IsAny<CommentEntity>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<CommentDto>(It.IsAny<CommentEntity>()), Times.Once);
        }

        [Fact]
        public async Task Handle_MappingReturnsNull_ThrowsCustomException()
        {
            // Arrange
            var commentUpdateDto = new CommentUpdateDto { Id = 1 };
            var command = new UpdateCommentCommand(commentUpdateDto);

            // Setup mapping to return null
            _mapperMock.Setup(m => m.Map<CommentEntity>(commentUpdateDto))
                .Returns((CommentEntity)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Cannot convert null to Comment", exception.Message);
            Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);

            _mapperMock.Verify(m => m.Map<CommentEntity>(commentUpdateDto), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.Update(It.IsAny<CommentEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_CommentNotFound_ThrowsCustomException()
        {
            // Arrange
            var commentUpdateDto = new CommentUpdateDto { Id = 1 };
            var commentEntityMapped = new CommentEntity { Id = 1 };
            var command = new UpdateCommentCommand(commentUpdateDto);

            // Setup mapping from CommentUpdateDto to CommentEntity
            _mapperMock.Setup(m => m.Map<CommentEntity>(commentUpdateDto))
                .Returns(commentEntityMapped);

            // Setup repository to return null (comment not found)
            _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()))
                .ReturnsAsync((CommentEntity)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            Assert.Equal($"Comment with id {commentEntityMapped.Id} not found", exception.Message);
            Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);

            _mapperMock.Verify(m => m.Map<CommentEntity>(commentUpdateDto), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.Update(It.IsAny<CommentEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SaveChangesFails_ThrowsCustomException()
        {
            // Arrange
            var commentUpdateDto = new CommentUpdateDto
            {
                Id = 1,
                CommentContent = "Updated comment"
            };

            var commentEntityMapped = new CommentEntity
            {
                Id = 1,
                CommentContent = "Updated comment"
            };

            var existingCommentEntity = new CommentEntity
            {
                Id = 1,
                CommentContent = "Original comment",
                DateCreated = DateTime.Now.AddDays(-1),
                StreetcodeId = 1,
                UserId = 1
            };

            var command = new UpdateCommentCommand(commentUpdateDto);

            // Setup mapping from CommentUpdateDto to CommentEntity
            _mapperMock.Setup(m => m.Map<CommentEntity>(commentUpdateDto))
                .Returns(commentEntityMapped);

            // Setup repository to return existing comment
            _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()))
                .ReturnsAsync(existingCommentEntity);

            // Setup SaveChangesAsync to fail
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Failed to update Comment", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);

            _mapperMock.Verify(m => m.Map<CommentEntity>(commentUpdateDto), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.Update(It.IsAny<CommentEntity>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
