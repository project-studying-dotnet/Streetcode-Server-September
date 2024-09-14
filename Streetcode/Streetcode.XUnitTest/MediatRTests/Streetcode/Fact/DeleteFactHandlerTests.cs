using Moq;
using Xunit;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Fact.Delete;

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
        var fact = new FactEntity { Id = factId };
        var command = new DeleteFactCommand(factId);

        var facts = new List<FactEntity> { fact };

        _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
            It.Is<Expression<Func<FactEntity, bool>>>(predicate => facts.Any(f => predicate.Compile()(f))),
            It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()
        ))
        .ReturnsAsync(fact);


        _repositoryWrapperMock.Setup(repo => repo.FactRepository.Delete(fact));

        _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.IsType<Unit>(result.Value);

        _repositoryWrapperMock.Verify(repo => repo.FactRepository.Delete(fact), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FactDoesNotExist_ReturnsFailure()
    {
        // Arrange
        int factId = 2;
        var command = new DeleteFactCommand(factId);

        var fact = new FactEntity { Id = 1 };
        var facts = new List<FactEntity> { fact };

        _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
            It.Is<Expression<Func<FactEntity, bool>>>(predicate => facts.Any(f => predicate.Compile()(f))),
            It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()
        ))
        .ReturnsAsync(fact);

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
    public async Task Handle_SaveChangesFails_ReturnsFailure()
    {
        // Arrange
        int factId = 1;
        var fact = new FactEntity { Id = factId };
        var command = new DeleteFactCommand(factId);

        var facts = new List<FactEntity> { fact };

        _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetFirstOrDefaultAsync(
            It.Is<Expression<Func<FactEntity, bool>>>(predicate => facts.Any(f => predicate.Compile()(f))),
            It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()
        ))
        .ReturnsAsync(fact);

        _repositoryWrapperMock.Setup(repo => repo.FactRepository.Delete(fact));

        _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0); // Simulate failure

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Failed to delete fact", result.Errors[0].Message);

        _repositoryWrapperMock.Verify(repo => repo.FactRepository.Delete(fact), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        _loggerMock.Verify(logger => logger.LogError(command, It.IsAny<string>()), Times.Once);
    }
}
