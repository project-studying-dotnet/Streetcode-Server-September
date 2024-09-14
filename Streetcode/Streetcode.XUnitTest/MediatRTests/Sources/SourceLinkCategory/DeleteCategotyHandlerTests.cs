using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Delete;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using SourceLinkCategoryEntity = Streetcode.DAL.Entities.Sources.SourceLinkCategory;

namespace Streetcode.XUnitTest.MediatRTests.Sources.SourceLinkCategory
{
    public class DeleteCategotyHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeleteCategoryHandler _handler;

        public DeleteCategotyHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new DeleteCategoryHandler(_repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenDTOFormed()
        {
            // Arrange
            var sourceLinkCategoryDTO = new SourceLinkCategoryDto();
            var query = new DeleteCategoryQuery(It.IsAny<int>());
            var sourceLinkCategory = new SourceLinkCategoryEntity();

            _repositoryMock.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(), null))
                .ReturnsAsync(sourceLinkCategory);

            _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<SourceLinkCategoryDto>(sourceLinkCategory))
                .Returns(sourceLinkCategoryDTO);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);  
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenCategoryNotFound()
        {
            // Arrange
            var sourceLinkCategoryDTO = new SourceLinkCategoryDto();
            var query = new DeleteCategoryQuery(It.IsAny<int>());
            var sourceLinkCategory = new SourceLinkCategoryEntity();

            _repositoryMock.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(), null))
                .ReturnsAsync((SourceLinkCategoryEntity)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("No category with such id", result.Errors.Select(e => e.Message));
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"No category with such id"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenSaveFailed()
        {
            // Arrange
            var sourceLinkCategoryDTO = new SourceLinkCategoryDto();
            var query = new DeleteCategoryQuery(It.IsAny<int>());
            var sourceLinkCategory = new SourceLinkCategoryEntity();

            _repositoryMock.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(), null))
                .ReturnsAsync(sourceLinkCategory);

            _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Failed to delete a related term", result.Errors.Select(e => e.Message));
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"Failed to delete a related term"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenMappingFailed()
        {
            // Arrange
            var sourceLinkCategoryDTO = new SourceLinkCategoryDto();
            var query = new DeleteCategoryQuery(It.IsAny<int>());
            var sourceLinkCategory = new SourceLinkCategoryEntity();

            _repositoryMock.Setup(r => r.SourceCategoryRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<SourceLinkCategoryEntity, bool>>>(), null))
                .ReturnsAsync(sourceLinkCategory);

            _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<SourceLinkCategoryEntity>(sourceLinkCategory))
                .Returns((SourceLinkCategoryEntity)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Failed to delete a related term", result.Errors.Select(e => e.Message));
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"Failed to delete a related term"), Times.Once);
        }
    }
}
