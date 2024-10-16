using AutoMapper;
using FluentAssertions;
using Streetcode.BLL.Dto.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Dto.Analytics;
using Streetcode.BLL.Mapping.AdditionalContent.Coordinates;
using Streetcode.BLL.Mapping.Analytics;
using Streetcode.DAL.Entities.Analytics;
using Xunit;

namespace Streetcode.XUnitTest.MappingTests.Analytics
{
    public class StatisticRecordProfileTests
    {
        private readonly IMapper _mapper;

        public StatisticRecordProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<StatisticRecordProfile>();
                cfg.AddProfile<StreetcodeCoordinateProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void StatisticRecord_ShouldMapTo_StatisticRecordDto()
        {
            // Arrange
            var record = new StatisticRecord
            {
                Id = 1,
                QrId = 99,
                Address = "Address",
                StreetcodeId = 1
            };

            // Act
            var recordDto = _mapper.Map<StatisticRecordDto>(record);

            // Assert
            recordDto.Should().NotBeNull();
            recordDto.Id.Should().Be(record.Id);
            recordDto.QrId.Should().Be(record.QrId);
            recordDto.Address.Should().Be(record.Address);
            recordDto.StreetcodeId.Should().Be(record.StreetcodeId);
        }

        [Fact]
        public void StatisticRecordDto_ShouldMapTo_StatisticRecord()
        {
            // Arrange
            var recordDto = new StatisticRecordDto
            {
                Id = 1,
                QrId = 99,
                Address = "Address",
                StreetcodeId = 1,
                StreetcodeCoordinateId = 2
            };

            // Act
            var record = _mapper.Map<StatisticRecordDto>(recordDto);

            // Assert
            record.Should().NotBeNull();
            record.Id.Should().Be(recordDto.Id);
            record.QrId.Should().Be(recordDto.QrId);
            record.Address.Should().Be(recordDto.Address);
            record.StreetcodeId.Should().Be(record.StreetcodeId);
            record.StreetcodeCoordinateId.Should().Be(record.StreetcodeCoordinateId);
        }

        [Fact]
        public void StatisticRecordCreateDto_ShouldMapTo_StatisticRecord()
        {
            // Arrange
            var recordCreateDto = new StatisticRecordCreateDto
            {
                QrId = 99,
                Address = "Address",
                StreetcodeCoordinate = new StreetcodeCoordinateDto()
                {
                    Id = 1,
                    StreetcodeId = 2
                }
            };

            // Act
            var record = _mapper.Map<StatisticRecord>(recordCreateDto);

            // Assert
            record.Should().NotBeNull();
            record.QrId.Should().Be(recordCreateDto.QrId);
            record.Address.Should().Be(recordCreateDto.Address);
            record.StreetcodeId.Should().Be(recordCreateDto.StreetcodeCoordinate.StreetcodeId);
            record.StreetcodeCoordinateId.Should().Be(recordCreateDto.StreetcodeCoordinate.Id);
        }
    }
}