using Xunit;
using Moq;
using AutoMapper;
using FluentResults;
using System.Threading;
using System.Threading.Tasks;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.GetById;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using FluentAssertions;
using System.Collections.Generic;
using Streetcode.DAL.Entities.Streetcode.TextContent;

using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.MediatRTests.Timeline.TimelineItem;

public class GetTimelineItemByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetTimelineItemByIdHandler _handler;

    public GetTimelineItemByIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetTimelineItemByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_TimelineItemNotFound_ShouldReturnFailResult()
    {
        // Arrange
        var request = new GetTimelineItemByIdQuery(1);
        _repositoryWrapperMock.Setup(repo => repo.TimelineRepository
            .GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
            .ReturnsAsync((TimelineItemEntity)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == $"Cannot find a timeline item with corresponding id: {request.Id}");
        _loggerMock.Verify(logger => logger.LogError(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TimelineItemFound_ShouldReturnOkResult()
    {
        // Arrange
        var request = new GetTimelineItemByIdQuery(1);
        var timelineItem = new TimelineItemEntity { Id = 1, Title = "Test Timeline" };
        var timelineItemDto = new TimelineItemDto { Id = 1, Title = "Test Timeline" };

        _repositoryWrapperMock.Setup(repo => repo.TimelineRepository
            .GetFirstOrDefaultAsync(
                It.Is<Expression<Func<TimelineItemEntity, bool>>>(exp => exp.Compile().Invoke(timelineItem) && timelineItem.Id == request.Id),
                It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
            .ReturnsAsync(timelineItem);

        _mapperMock.Setup(m => m.Map<TimelineItemDto>(timelineItem)).Returns(timelineItemDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(timelineItemDto);
        _mapperMock.Verify(m => m.Map<TimelineItemDto>(timelineItem), Times.Once);
    }

}
