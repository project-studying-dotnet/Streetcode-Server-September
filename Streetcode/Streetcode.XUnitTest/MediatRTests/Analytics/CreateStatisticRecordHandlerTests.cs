using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Dto.Analytics;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Analytics.Create;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using StreetcodeCoordinateDto = Streetcode.BLL.Dto.AdditionalContent.Coordinates.Types.StreetcodeCoordinateDto;

namespace Streetcode.XUnitTest.MediatRTests.Analytics
{
    public class CreateStatisticRecordHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly CreateStatisticRecordHandler _handler;

        public CreateStatisticRecordHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new CreateStatisticRecordHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenStatisticRecordIsCreatedSuccessfully()
        {
            // Arrange
            var statisticRecordCreateDto = new StatisticRecordCreateDto
            {
                QrId = 99,
                Address = "Address",
                StreetcodeCoordinate = new StreetcodeCoordinateDto()
                {
                    StreetcodeId = 1
                }
            };
            var statisticRecordDto = new StatisticRecordDto
            {
                Id = 1,
                QrId = 99,
                Address = "Address",
                StreetcodeId = 1
            };

            var statisticRecord = new StatisticRecord() 
            {
                QrId = 99,
                Address = "Address",
                StreetcodeId = 1 
            };
            var statisticRecords = new List<StatisticRecord>() { new StatisticRecord() { QrId = 9, StreetcodeId = 1 } };

            var command = new CreateStatisticRecordCommand(statisticRecordCreateDto);


            _mapperMock.Setup(m => m.Map<StatisticRecord>(command.newStreetcodeRecord)).Returns(statisticRecord);

            _repositoryWrapperMock.Setup(r => r.StatisticRecordRepository.GetAllAsync(
                It.IsAny<Expression<Func<StatisticRecord, bool>>>(), null)).ReturnsAsync(statisticRecords);
            _repositoryWrapperMock.Setup(r => r.StatisticRecordRepository.CreateAsync(statisticRecord)).ReturnsAsync(statisticRecord);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<StatisticRecordDto>(statisticRecord)).Returns(statisticRecordDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(statisticRecordCreateDto.QrId, result.Value.QrId);
            Assert.Equal(statisticRecordCreateDto.Address, result.Value.Address);
            Assert.Equal(statisticRecordCreateDto.StreetcodeCoordinate.StreetcodeId, result.Value.StreetcodeId);

        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenQrIdAlreadyExists()
        {
            // Arrange
            var statisticRecordDto = new StatisticRecordCreateDto 
            { 
                QrId = 99,
                StreetcodeCoordinate = new StreetcodeCoordinateDto() 
                { 
                    StreetcodeId = 1
                } 
            };

            var statisticRecord = new StatisticRecord() { QrId = 99, StreetcodeId = 1 };
            var statisticRecords = new List<StatisticRecord>() { statisticRecord };

            var command = new CreateStatisticRecordCommand(statisticRecordDto);
            string errorMsg = $"A QR table number of { statisticRecordDto.QrId} already exists for this streetcode. " +
                               "Please choose a different number.";

            _mapperMock.Setup(m => m.Map<StatisticRecord>(command.newStreetcodeRecord)).Returns(statisticRecord);

            _repositoryWrapperMock.Setup(r => r.StatisticRecordRepository.GetAllAsync(
                It.IsAny<Expression<Func<StatisticRecord, bool>>>(), null)).ReturnsAsync(statisticRecords);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(errorMsg, result.Errors.First().Message);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenMappingReturnsNull()
        {
            // Arrange
            var statisticRecordDto = new StatisticRecordCreateDto();
            var request = new CreateStatisticRecordCommand(statisticRecordDto);

            _mapperMock.Setup(m => m.Map<StatisticRecord>(request.newStreetcodeRecord)).Returns((StatisticRecord)null!);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot convert null to a Statistic Record", result.Errors[0].Message);
            _repositoryWrapperMock.Verify(r => r.StatisticRecordRepository.CreateAsync(It.IsAny<StatisticRecord>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowCustomException_WhenSaveChangesFails()
        {
            // Arrange
            var statisticRecordDto = new StatisticRecordCreateDto
            {
                QrId = 99,
                StreetcodeCoordinate = new StreetcodeCoordinateDto()
                {
                    StreetcodeId = 1
                }
            };

            var statisticRecord = new StatisticRecord() { QrId = 99, StreetcodeId = 1 };
            var statisticRecords = new List<StatisticRecord>() { new StatisticRecord() { QrId = 9, StreetcodeId = 1 } };

            var command = new CreateStatisticRecordCommand(statisticRecordDto);
            var errorMsg = "Failed to save a Statistic Record";

            _mapperMock.Setup(m => m.Map<StatisticRecord>(command.newStreetcodeRecord)).Returns(statisticRecord);

            _repositoryWrapperMock.Setup(repo => repo.StatisticRecordRepository.GetAllAsync(
                It.IsAny<Expression<Func<StatisticRecord, bool>>>(), null)).ReturnsAsync(statisticRecords);
            _repositoryWrapperMock.Setup(repo => repo.StatisticRecordRepository.CreateAsync(statisticRecord)).ReturnsAsync(statisticRecord);
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var exception = await Assert.ThrowsAsync<CustomException>(
                () => _handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal(errorMsg, exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }
    }
}
