using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using TimelineEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.MediatRTests.Timeline.TimelineItem
{
    public class DeleteTimelineItemHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeleteTimelineItemHandler _handler;

        public DeleteTimelineItemHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new DeleteTimelineItemHandler(_repositoryWrapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_TimelineItemExists_ShouldReturnSuccessResult()
        {
            // Arrange
            var timelineItem = new TimelineEntity { Id = 1 };
            var command = new DeleteTimelineItemCommand(1);

            _repositoryWrapperMock
                .Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineEntity>, IIncludableQueryable<TimelineEntity, object>>>()))
                .ReturnsAsync(timelineItem);

            _repositoryWrapperMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Unit.Value, result.Value);
        }

        [Fact]
        public async Task Handle_TimelineItemNotFound_ShouldReturnFailureResult()
        {
            // Arrange
            var command = new DeleteTimelineItemCommand(1); 

            _repositoryWrapperMock
                .Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineEntity>, IIncludableQueryable<TimelineEntity, object>>>()))
                .ReturnsAsync((TimelineEntity)null!);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot find timeline item", result.Errors[0].Message);
            _loggerMock.Verify(x => x.LogError(It.IsAny<object>(), "Cannot find timeline item"), Times.Once);
        }

        [Fact]
        public async Task Handle_FailedToDeleteTimelineItem_ShouldReturnFailureResult()
        {
            // Arrange
            var timelineItem = new TimelineEntity { Id = 1 };
            var command = new DeleteTimelineItemCommand(1); 

            _repositoryWrapperMock
                .Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineEntity>, IIncludableQueryable<TimelineEntity, object>>>()))
                .ReturnsAsync(timelineItem);

            _repositoryWrapperMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(0); 

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot delete timeline item", result.Errors[0].Message);
            _loggerMock.Verify(x => x.LogError(It.IsAny<object>(), "Cannot delete timeline item"), Times.Once);
        }

    }
}
