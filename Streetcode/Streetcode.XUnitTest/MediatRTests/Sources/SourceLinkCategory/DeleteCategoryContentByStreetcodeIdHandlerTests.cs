using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Delete;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
using Streetcode.DAL.Entities.Sources;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
namespace Streetcode.XUnitTest.MediatRTests.Sources.SourceLinkCategory
{
    using StreetcodeCategoryContent = DAL.Entities.Sources.StreetcodeCategoryContent;
    public class DeleteCategoryHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeleteCategoryContentByStreetcodeIdHandler _handler;

        public DeleteCategoryHandlerTests()
        {
            _repositoryMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new DeleteCategoryContentByStreetcodeIdHandler(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenContentFound()
        {
            // Arrange
            var query = new DeleteCategoryContentByStreetcodeIdQuery(It.IsAny<int>(), It.IsAny<int>());
            var sourceLinkCategoryContent = new StreetcodeCategoryContent { SourceLinkCategoryId = 1, StreetcodeId = 1 };

            _repositoryMock.Setup(r => r.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCategoryContent, bool>>>(), null))
             .ReturnsAsync(sourceLinkCategoryContent);

            _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            _repositoryMock.Verify(r => r.StreetcodeCategoryContentRepository.Delete(sourceLinkCategoryContent), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenContentNotFound()
        {
            // Arrange
            var query = new DeleteCategoryContentByStreetcodeIdQuery(It.IsAny<int>(), It.IsAny<int>());

            _repositoryMock.Setup(r => r.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCategoryContent, bool>>>(), null))
             .ReturnsAsync((StreetcodeCategoryContent) null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("No item with such ids", result.Errors.Select(e => e.Message));
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"No item with such ids"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenSaveFailed()
        {
            // Arrange
            var query = new DeleteCategoryContentByStreetcodeIdQuery(It.IsAny<int>(), It.IsAny<int>());
            var sourceLinkCategoryContent = new StreetcodeCategoryContent { SourceLinkCategoryId = 1, StreetcodeId = 1 };

            _repositoryMock.Setup(r => r.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCategoryContent, bool>>>(), null))
             .ReturnsAsync(sourceLinkCategoryContent);

            _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Failed to delete item", result.Errors.Select(e => e.Message));
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"Failed to delete item"), Times.Once);
        }
    }
}
