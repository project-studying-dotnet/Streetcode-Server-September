using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Analytics.Delete;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Analytics;

public class DeleteStatisticRecordHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly DeleteStatisticRecordHandler _handler;

    public DeleteStatisticRecordHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _handler = new DeleteStatisticRecordHandler(_repositoryWrapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenArtNotFound()
    {
        // Arrange
        var statisticRecordId = 1;
        var command = new DeleteStatisticRecordCommand(statisticRecordId);
        var errorMsg = $"Cannot find a Statistic Record by entered Id: {statisticRecordId}";

        _repositoryWrapperMock.Setup(repo => repo.StatisticRecordRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StatisticRecord, bool>>>(), null))
            .ReturnsAsync((StatisticRecord)null!);

        _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var exception = await Assert.ThrowsAsync<CustomException>(
                () =>  _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(errorMsg, exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        _repositoryWrapperMock.Verify(repo => repo.StatisticRecordRepository.Delete(It.IsAny<StatisticRecord>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenSaveChangesFailed()
    {
        // Arrange
        var statisticRecordId = 1;
        var statisticRecord = new StatisticRecord { Id = 1, StreetcodeCoordinateId = 10 };

        var command = new DeleteStatisticRecordCommand(statisticRecordId);
        var errorMsg = "Failed to delete a Statistic Record";

        _repositoryWrapperMock.Setup(repo => repo.StatisticRecordRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StatisticRecord, bool>>>(), null))
            .ReturnsAsync(statisticRecord);

        _repositoryWrapperMock.Setup(repo => repo.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(new StreetcodeCoordinate { Id = 10 });

        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var exception = await Assert.ThrowsAsync<CustomException>(
               () => _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(errorMsg, exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_ReturnSuccess_WhenStreetcodeRecordFound()
    {
        // Arrange
        var statisticRecordId = 1;
        var statisticRecord = new StatisticRecord() { Id = 1, StreetcodeCoordinateId = 1 };
        var streetcodeCoordinate = new StreetcodeCoordinate() { Id = 1 };
        var command = new DeleteStatisticRecordCommand(statisticRecordId);

        _repositoryWrapperMock.Setup(repo => repo.StatisticRecordRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StatisticRecord, bool>>>(), null))
            .ReturnsAsync(statisticRecord);
        _repositoryWrapperMock.Setup(repo => repo.StreetcodeCoordinateRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCoordinate, bool>>>(), null))
            .ReturnsAsync(streetcodeCoordinate);

        _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await  _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryWrapperMock.Verify(repo => repo.StreetcodeCoordinateRepository.Delete(streetcodeCoordinate), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.StatisticRecordRepository.Delete(statisticRecord), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}
