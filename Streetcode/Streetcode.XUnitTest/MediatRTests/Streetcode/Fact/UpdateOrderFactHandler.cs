using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder;
using Streetcode.DAL.Repositories.Interfaces.Base;

using FactEntety = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Fact;

public class UpdateOrderFactHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly UpdateOrderFactHandler _handler;

    public UpdateOrderFactHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();

        _handler = new UpdateOrderFactHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_SuccessfulUpdate_ReturnsUpdatedFacts()
    {
        // Arrange
        var factIdToMove = 2;
        var newOrder = 1;
        var streetcodeId = 1;

        var requestDto = new FactOrderUpdateDto
        {
            FactId = factIdToMove,
            NewOrder = newOrder,
            StreetcodeId = streetcodeId
        };

        var command = new UpdateOrderFactCommand(requestDto);

        var facts = new List<FactEntety>
        {
            new FactEntety { Id = 1, StreetcodeId = streetcodeId, SortOrder = 1 },
            new FactEntety { Id = 2, StreetcodeId = streetcodeId, SortOrder = 2 },
            new FactEntety { Id = 3, StreetcodeId = streetcodeId, SortOrder = 3 }
        };

        _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FactEntety, bool>>>(),
            null))
            .ReturnsAsync(facts);

        _repositoryWrapperMock.Setup(repo => repo.FactRepository.UpdateRange(facts));

        _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<FactDto>>(It.IsAny<IEnumerable<FactEntety>>()))
            .Returns((IEnumerable<FactEntety> source) => source.Select(f => new FactDto
            {
                Id = f.Id,
                SortOrder = f.SortOrder
            }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updatedFacts = result.Value.ToList();
        Assert.Equal(3, updatedFacts.Count);

        // Verify that the fact orders have been updated correctly
        Assert.Equal(2, updatedFacts.First(f => f.Id == 1).SortOrder);
        Assert.Equal(1, updatedFacts.First(f => f.Id == 2).SortOrder);
        Assert.Equal(3, updatedFacts.First(f => f.Id == 3).SortOrder);

        _repositoryWrapperMock.Verify(repo => repo.FactRepository.UpdateRange(facts), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoFactsFound_ReturnsFailure()
    {
        // Arrange
        var streetcodeId = 1;
        var requestDto = new FactOrderUpdateDto
        {
            FactId = 1,
            NewOrder = 1,
            StreetcodeId = streetcodeId
        };

        var command = new UpdateOrderFactCommand(requestDto);

        // Mock repository to return null or empty list
        _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FactEntety, bool>>>(),
            null))
            .ReturnsAsync(new List<FactEntety>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Facts not found", result.Errors.First().Message);

        _loggerMock.Verify(logger => logger.LogError(command, $"No facts found for StreetcodeId: {streetcodeId}"), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.FactRepository.UpdateRange(It.IsAny<IEnumerable<FactEntety>>()), Times.Never);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ExceptionOccurs_ReturnsFailure()
    {
        // Arrange
        var factIdToMove = 5; // Fact ID that does not exist in the facts list
        var newOrder = 1;
        var streetcodeId = 1;

        var requestDto = new FactOrderUpdateDto
        {
            FactId = factIdToMove,
            NewOrder = newOrder,
            StreetcodeId = streetcodeId
        };

        var command = new UpdateOrderFactCommand(requestDto);

        var facts = new List<FactEntety>
        {
            new FactEntety { Id = 1, StreetcodeId = streetcodeId, SortOrder = 1 },
            new FactEntety { Id = 2, StreetcodeId = streetcodeId, SortOrder = 2 },
            new FactEntety { Id = 3, StreetcodeId = streetcodeId, SortOrder = 3 }
        };

        _repositoryWrapperMock.Setup(repo => repo.FactRepository.GetAllAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<FactEntety, bool>>>(),
            null))
            .ReturnsAsync(facts);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error occurred while updating fact order", result.Errors.First().Message);

        _loggerMock.Verify(logger => logger.LogError(
            command,
            It.Is<string>(msg => msg.Contains("Error occurred while updating fact order"))),
            Times.Once);

        _repositoryWrapperMock.Verify(repo => repo.FactRepository.UpdateRange(It.IsAny<IEnumerable<FactEntety>>()), Times.Never);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}
