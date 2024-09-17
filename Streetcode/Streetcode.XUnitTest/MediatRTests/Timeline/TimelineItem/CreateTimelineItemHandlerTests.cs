using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Timeline.TimelineItem.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TimelineItemEntity = Streetcode.DAL.Entities.Timeline.TimelineItem;
using StreetcodeEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;
using System.Linq.Expressions;


namespace Streetcode.XUnitTest.MediatRTests.Timeline.TimelineItem
{
    public class CreateTimelineItemHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly CreateTimelineItemHandler _handler;

        public CreateTimelineItemHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new CreateTimelineItemHandler(_mapperMock.Object, _repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessResult_WhenTimelineItemIsCreatedSuccessfully()
        {
            // Arrange
            var timelineItemCreateDto = new TimelineItemCreateDto
            {
                Title = "Test Timeline",
                Description = "Test description",
                Date = new DateTime(2024, 9, 17),
                DateViewPattern = 0,
                StreetcodeId = 99
            };

            var timelineItemDto = new TimelineItemDto
            {
                Id = 1,
                Title = "Test Timeline",
                Description = "Test description",
                Date = new DateTime(2024, 9, 17),
                DateViewPattern = 0
            };

            var timelineItemEntity = new TimelineItemEntity
            {
                Title = "Test Timeline",
                Description = "Test description",
                Date = new DateTime(2024, 9, 17),
                DateViewPattern = 0,
                StreetcodeId = 99
            };

            var streetcodeEntity = new StreetcodeEntity { Id = 99 };

            var command = new CreateTimelineItemCommand(timelineItemCreateDto);

            _repositoryMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                  It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(), null))
              .ReturnsAsync(streetcodeEntity);

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(timelineItemCreateDto)).Returns(timelineItemEntity);
            _repositoryMock.Setup(r => r.TimelineRepository.CreateAsync(timelineItemEntity)).ReturnsAsync(timelineItemEntity);
            _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<TimelineItemDto>(timelineItemEntity)).Returns(timelineItemDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(timelineItemCreateDto.Title, result.Value.Title);
            Assert.Equal(timelineItemCreateDto.Description, result.Value.Description);
            Assert.Equal(timelineItemCreateDto.Date, result.Value.Date);
            Assert.Equal(timelineItemCreateDto.DateViewPattern, result.Value.DateViewPattern);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenStreetcodeDoesNotExist()
        {
            // Arrange
            var timelineItemCreateDto = new TimelineItemCreateDto
            {
                Title = "Test Title",
                Description = "Test Description",
                Date = DateTime.Now,
                DateViewPattern = 0,
                StreetcodeId = 0
            };

            var command = new CreateTimelineItemCommand(timelineItemCreateDto);

            _repositoryMock
              .Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                  It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(), null))
              .ReturnsAsync((StreetcodeEntity)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Streetcode does not exist.", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenSaveChangesFails()
        {
            // Arrange
            var timelineItemCreateDto = new TimelineItemCreateDto { };               
            var streetcodeEntity = new StreetcodeEntity { Id = 99 };
            var timelineItemEntity = new TimelineItemEntity { StreetcodeId = 99 };

            var command = new CreateTimelineItemCommand(timelineItemCreateDto);

            _repositoryMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                  It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(), null))
              .ReturnsAsync(streetcodeEntity);

            _mapperMock.Setup(m => m.Map<TimelineItemEntity>(timelineItemCreateDto)).Returns(timelineItemEntity);
            _repositoryMock.Setup(r => r.TimelineRepository.CreateAsync(timelineItemEntity)).ReturnsAsync(timelineItemEntity);
            _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0); 

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to create TimelineItem", result.Errors.First().Message);
        }
    }
}
