using AutoMapper;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Comment.GetByStreetcodeId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetByStreetcodeId;
using CommentEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Comment;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Comment.GetByStreetcodeId
{
    public class GetCommentByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetCommentByStreetcodeIdHandler _handler;

        public GetCommentByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetCommentByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_CommentsFound_ReturnsSuccessResult()
        {
            // Arrange
            int streetcodeId = 1;
            var comments = new List<CommentEntity>
            {
                new CommentEntity { Id = 1, CommentContent = "Comment 1", StreetcodeId = streetcodeId },
                new CommentEntity { Id = 2, CommentContent = "Comment 2", StreetcodeId = streetcodeId }
            };
            var commentDtos = new List<CommentDto>
            {
                new CommentDto { Id = 1, CommentContent = "Comment 1", StreetcodeId = streetcodeId },
                new CommentDto { Id = 2, CommentContent = "Comment 2", StreetcodeId = streetcodeId }
            };
            var query = new GetCommentByStreetcodeIdQuery(streetcodeId);

            _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()))
                .ReturnsAsync(comments);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<CommentDto>>(comments))
                .Returns(commentDtos);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(commentDtos, result.Value);

            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Once);

            _mapperMock.Verify(mapper => mapper.Map<IEnumerable<CommentDto>>(comments), Times.Once);
        }

        [Fact]
        public async Task Handle_CommentsNotFound_ReturnsEmptyList()
        {
            // Arrange
            int streetcodeId = 1;
            var comments = new List<CommentEntity>(); // Пустой список
            var commentDtos = new List<CommentDto>(); // Пустой список
            var query = new GetCommentByStreetcodeIdQuery(streetcodeId);

            _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()))
                .ReturnsAsync(comments);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<CommentDto>>(comments))
                .Returns(commentDtos);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);

            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Once);

            _mapperMock.Verify(mapper => mapper.Map<IEnumerable<CommentDto>>(comments), Times.Once);
        }

        [Fact]
        public async Task Handle_GetAllAsyncReturnsNull_ThrowsCustomException()
        {
            // Arrange
            int streetcodeId = 1;
            var query = new GetCommentByStreetcodeIdQuery(streetcodeId);

            _repositoryWrapperMock.Setup(repo => repo.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()))
                .ReturnsAsync((IEnumerable<CommentEntity>)null); // Возвращаем null

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(query, CancellationToken.None));

            Assert.Equal($"Cannot find any comment by the streetcode id: {streetcodeId}", exception.Message);
            Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);

            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.GetAllAsync(
                It.IsAny<Expression<Func<CommentEntity, bool>>>(),
                It.IsAny<Func<IQueryable<CommentEntity>, IIncludableQueryable<CommentEntity, object>>>()), Times.Once);

            _mapperMock.Verify(mapper => mapper.Map<IEnumerable<CommentDto>>(It.IsAny<IEnumerable<CommentEntity>>()), Times.Never);
        }
    }
}
