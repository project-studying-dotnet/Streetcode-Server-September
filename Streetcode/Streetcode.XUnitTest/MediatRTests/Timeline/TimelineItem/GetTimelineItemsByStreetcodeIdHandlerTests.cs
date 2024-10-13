using Xunit;
using Moq;
using AutoMapper;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetByStreetcodeId;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Interfaces.Logging;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using FluentAssertions;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.MediatRTests.Timeline.TimelineItem;

public class GetTimelineItemsByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetTimelineItemsByStreetcodeIdHandler _handler;

    public GetTimelineItemsByStreetcodeIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetTimelineItemsByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_TimelineItemsNotFound_ShouldReturnFailResult()
    {
        // Arrange
        var request = new GetTimelineItemsByStreetcodeIdQuery(1);
        _repositoryWrapperMock.Setup(repo => repo.TimelineRepository
            .GetAllAsync(
                It.Is<Expression<Func<TimelineItemEntity, bool>>>(exp => exp.Compile().Invoke(new TimelineItemEntity { StreetcodeId = request.StreetcodeId })),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))!
            .ReturnsAsync((IEnumerable<TimelineItemEntity>)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == $"Cannot find any timeline item by the streetcode id: {request.StreetcodeId}");
        _loggerMock.Verify(logger => logger.LogError(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TimelineItemsFound_ShouldReturnOkResult()
    {
        // Arrange
        var request = new GetTimelineItemsByStreetcodeIdQuery(1);
        var timelineItems = new List<TimelineItemEntity>
        {
            new TimelineItemEntity { Id = 1, Title = "Test Timeline", StreetcodeId = 1 },
            new TimelineItemEntity { Id = 2, Title = "Another Timeline", StreetcodeId = 1 },
        };

        var timelineItemDtos = new List<TimelineItemDto>
        {
            new TimelineItemDto { Id = 1, Title = "Test Timeline" },
            new TimelineItemDto { Id = 2, Title = "Another Timeline" },
        };

        _repositoryWrapperMock.Setup(r => r.TimelineRepository.GetAllAsync(
            It.Is<Expression<Func<TimelineItemEntity, bool>>>(expr =>
                timelineItems.All(item => expr.Compile()(item) && item.StreetcodeId == request.StreetcodeId)),
            It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
            .ReturnsAsync(timelineItems);

        _mapperMock.Setup(m => m.Map<IEnumerable<TimelineItemDto>>(timelineItems)).Returns(timelineItemDtos);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(timelineItemDtos, result.Value);
        _mapperMock.Verify(m => m.Map<IEnumerable<TimelineItemDto>>(timelineItems), Times.Once);
    }
}