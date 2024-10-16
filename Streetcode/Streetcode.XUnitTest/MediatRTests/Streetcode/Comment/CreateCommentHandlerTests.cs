using AutoMapper;
using Moq;
using Xunit;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Comment.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using commentEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Comment;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Comment.Create
{
    public class CreateCommentHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly CreateCommentHandler _handler;

        public CreateCommentHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new CreateCommentHandler(_mapperMock.Object, _repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var commentCreateDto = new CommentCreateDto
            {
                CommentContent = "Test comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = DateTime.Now
            };
            var command = new CreateCommentCommand(commentCreateDto);
            var commentEntity = new commentEntity
            {
                CommentContent = "Test comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = commentCreateDto.DateCreated
            };
            var createdEntity = new commentEntity
            {
                Id = 1,
                CommentContent = "Test comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = commentCreateDto.DateCreated
            };
            var expectedCommentDto = new CommentDto
            {
                Id = 1,
                CommentContent = "Test comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = commentCreateDto.DateCreated
            };

            _mapperMock.Setup(mapper => mapper.Map<commentEntity>(commentCreateDto))
                .Returns(commentEntity);

            _repositoryWrapperMock.Setup(repo => repo.CommentRepository.CreateAsync(commentEntity))
                .ReturnsAsync(createdEntity);

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            _mapperMock.Setup(mapper => mapper.Map<CommentDto>(createdEntity))
                .Returns(expectedCommentDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedCommentDto, result.Value);
            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.CreateAsync(commentEntity), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_MappingReturnsNull_ThrowsCustomException()
        {
            // Arrange
            var commentCreateDto = new CommentCreateDto
            {
                CommentContent = "Test comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = DateTime.Now
            };
            var command = new CreateCommentCommand(commentCreateDto);

            _mapperMock.Setup(mapper => mapper.Map<commentEntity>(commentCreateDto))
                .Returns((commentEntity)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Cannot convert null to comment", exception.Message);
            Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);

            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.CreateAsync(It.IsAny<commentEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SaveChangesFails_ThrowsCustomException()
        {
            // Arrange
            var commentCreateDto = new CommentCreateDto
            {
                CommentContent = "Test comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = DateTime.Now
            };
            var command = new CreateCommentCommand(commentCreateDto);
            var commentEntity = new commentEntity
            {
                CommentContent = "Test comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = commentCreateDto.DateCreated
            };
            var createdEntity = new commentEntity
            {
                Id = 1,
                CommentContent = "Test comment",
                StreetcodeId = 1,
                UserId = 1,
                DateCreated = commentCreateDto.DateCreated
            };

            _mapperMock.Setup(mapper => mapper.Map<commentEntity>(commentCreateDto))
                .Returns(commentEntity);

            _repositoryWrapperMock.Setup(repo => repo.CommentRepository.CreateAsync(commentEntity))
                .ReturnsAsync(createdEntity);

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
            Assert.Equal("Failed to create a comment", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);

            _repositoryWrapperMock.Verify(repo => repo.CommentRepository.CreateAsync(commentEntity), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
