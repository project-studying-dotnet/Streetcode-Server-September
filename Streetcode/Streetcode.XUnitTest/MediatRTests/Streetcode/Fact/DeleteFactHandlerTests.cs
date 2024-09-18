using Moq;
using Xunit;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Fact.Delete
{
    public class DeleteFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeleteFactHandler _handler;

        public DeleteFactHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new DeleteFactHandler(_repositoryWrapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_FactExists_DeletesFactAndReturnsSuccess()
        {
            // Arrange
            int factId = 1;
            int streetcodeId = 1;
            var fact = new FactEntity { Id = factId, StreetcodeId = streetcodeId, SortOrder = 1 };
            var command = new DeleteFactCommand(factId);

            var facts = new List<FactEntity>
            {
                fact,
                new FactEntity { Id = 2, StreetcodeId = streetcodeId, SortOrder = 2 },
                new FactEntity { Id = 3, StreetcodeId = streetcodeId, SortOrder = 3 }
            };

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync(fact);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetAllAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync(facts);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.Delete(fact));

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.UpdateRange(It.IsAny<IEnumerable<FactEntity>>()));

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<Unit>(result.Value);

            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Delete(fact), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.FactRepository.UpdateRange(It.IsAny<IEnumerable<FactEntity>>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_FactDoesNotExist_ReturnsFailure()
        {
            // Arrange
            int factId = 2;
            var command = new DeleteFactCommand(factId);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync((FactEntity)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal($"No fact found by entered Id - {factId}", result.Errors[0].Message);

            _loggerMock.Verify(logger => logger.LogError(command, It.IsAny<string>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Delete(It.IsAny<FactEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_NoFactsForStreetcode_ReturnsFailure()
        {
            // Arrange
            int factId = 1;
            int streetcodeId = 1;
            var fact = new FactEntity { Id = factId, StreetcodeId = streetcodeId };
            var command = new DeleteFactCommand(factId);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync(fact);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetAllAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync((IEnumerable<FactEntity>)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal($"No facts found for StreetcodeId: {fact.StreetcodeId}", result.Errors[0].Message);

            _loggerMock.Verify(logger => logger.LogError(command, It.IsAny<string>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Delete(It.IsAny<FactEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_FactsListIsEmpty_ReturnsFailure()
        {
            // Arrange
            int factId = 1;
            int streetcodeId = 1;
            var fact = new FactEntity { Id = factId, StreetcodeId = streetcodeId };
            var command = new DeleteFactCommand(factId);

            var facts = new List<FactEntity>();

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync(fact);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetAllAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync(facts);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal($"No facts found for StreetcodeId: {fact.StreetcodeId}", result.Errors[0].Message);

            _loggerMock.Verify(logger => logger.LogError(command, It.IsAny<string>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Delete(It.IsAny<FactEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SaveChangesFails_ReturnsFailure()
        {
            // Arrange
            int factId = 1;
            int streetcodeId = 1;
            var fact = new FactEntity { Id = factId, StreetcodeId = streetcodeId, SortOrder = 1 };
            var command = new DeleteFactCommand(factId);

            var facts = new List<FactEntity>
            {
                fact,
                new FactEntity { Id = 2, StreetcodeId = streetcodeId, SortOrder = 2 }
            };

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync(fact);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetAllAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync(facts);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.Delete(fact));

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(0); // Симулируем ошибку сохранения

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to delete fact", result.Errors[0].Message);

            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Delete(fact), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(logger => logger.LogError(command, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_FactExists_OrderUpdatedCorrectly()
        {
            // Arrange
            int factId = 2;
            int streetcodeId = 1;
            var factToDelete = new FactEntity { Id = factId, StreetcodeId = streetcodeId, SortOrder = 2 };
            var command = new DeleteFactCommand(factId);

            var facts = new List<FactEntity>
            {
                new FactEntity { Id = 1, StreetcodeId = streetcodeId, SortOrder = 1 },
                factToDelete,
                new FactEntity { Id = 3, StreetcodeId = streetcodeId, SortOrder = 3 },
                new FactEntity { Id = 4, StreetcodeId = streetcodeId, SortOrder = 4 }
            };

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync(factToDelete);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetAllAsync(
                It.IsAny<Expression<Func<FactEntity, bool>>>(),
                null))
                .ReturnsAsync(facts);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.Delete(factToDelete));

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.UpdateRange(It.IsAny<IEnumerable<FactEntity>>()));

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<Unit>(result.Value);

            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Delete(factToDelete), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.FactRepository.UpdateRange(It.Is<IEnumerable<FactEntity>>(updatedFacts =>
                updatedFacts.Any(f => f.Id == 3 && f.SortOrder == 2) &&
                updatedFacts.Any(f => f.Id == 4 && f.SortOrder == 3)
            )), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }
    }
}
