using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Update;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;

namespace Streetcode.XUnitTest.MediatRTests.Timeline.TimelineItem
{
    public class UpdateTimelineItemHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly UpdateTimelineItemHandler _handler;

        public UpdateTimelineItemHandlerTests()
        {
            _repositoryMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new UpdateTimelineItemHandler(_mapperMock.Object, _repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_TimelineItemNotFound_ShouldReturnFailureResult()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync((TimelineItemEntity)null!);

            var command = new CreateTimelineItemCommand(new TimelineItemUpdateDto { Id = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_TimelineItemFound_ShouldUpdateAndReturnSuccess()
        {
            // Arrange
            var existingItem = new TimelineItemEntity { Id = 1 };

            _repositoryMock
                .Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync(existingItem);

            _repositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            var updateDto = new TimelineItemUpdateDto { Id = 1 };
            var command = new CreateTimelineItemCommand(updateDto);
            var updatedDto = new TimelineItemDto { Id = 1 };

            _mapperMock.Setup(mapper => mapper.Map<TimelineItemDto>(It.IsAny<TimelineItemEntity>())).Returns(updatedDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(updatedDto, result.Value);
            _repositoryMock.Verify(repo => repo.TimelineRepository.Update(It.IsAny<TimelineItemEntity>()), Times.Once);
            _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_SaveOperationFails_ShouldReturnFailureResult()
        {
            // Arrange
            var existingItem = new TimelineItemEntity { Id = 1 };

            _repositoryMock
                .Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync(existingItem);

            _repositoryMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(0);

            var command = new CreateTimelineItemCommand(new TimelineItemUpdateDto { Id = 1 });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldLogWhenItemNotFound()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.TimelineRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<TimelineItemEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<TimelineItemEntity>, IIncludableQueryable<TimelineItemEntity, object>>>()))
                .ReturnsAsync((TimelineItemEntity)null!);

            var command = new CreateTimelineItemCommand(new TimelineItemUpdateDto { Id = 1 });

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }
    }
}