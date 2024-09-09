using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.MediatRTests.Timeline.TimelineItem;

public class GetAllTimelineItemsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetAllTimelineItemsHandler _handler;

    public GetAllTimelineItemsHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetAllTimelineItemsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_TimelineItemsNotFound_ShouldReturnFailResult()
    {
        // Arrange
        _repositoryWrapperMock.Setup(repo => repo.TimelineRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync((IEnumerable<TimelineItemEntity>)null);

        // Act
        var result = await _handler.Handle(new GetAllTimelineItemsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == "Cannot find any timelineItem");
        _loggerMock.Verify(logger => logger.LogError(It.IsAny<GetAllTimelineItemsQuery>(), "Cannot find any timelineItem"), Times.Once);
    }

    [Fact]
    public async Task Handle_TimelineItemsFound_ShouldReturnOkResult()
    {
        // Arrange
        var timelineItems = new List<TimelineItemEntity>
            {
                new () { Id = 1, Title = "Event 1" },
                new () { Id = 2, Title = "Event 2" },
            };

        var timelineItemDtos = new List<TimelineItemDto>
            {
                new () { Id = 1, Title = "Event 1" },
                new () { Id = 2, Title = "Event 2" },
            };

        _repositoryWrapperMock.Setup(repo => repo.TimelineRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync(timelineItems);

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<TimelineItemDto>>(timelineItems))
            .Returns(timelineItemDtos);

        // Act
        var result = await _handler.Handle(new GetAllTimelineItemsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(timelineItemDtos, result.ValueOrDefault);
    }
}
