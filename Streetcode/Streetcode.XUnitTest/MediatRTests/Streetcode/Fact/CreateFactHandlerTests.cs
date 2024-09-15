using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Fact
{
    public class CreateFactHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IRepositoryWrapper> _mockRepository;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly CreateFactHandler _handler;

        public CreateFactHandlerTests()
        {
            _mockMapper = new Mock<IMapper>();
            _mockRepository = new Mock<IRepositoryWrapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new CreateFactHandler(_mockMapper.Object, _mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessResult_WhenFactIsCreatedSuccessfully()
        {
            // Arrange
            var factCreateDto = new FactCreateDto
            {
                Title = "Test Fact",
                ImageId = 1,
                FactContent = "Some content",
                StreetcodeId = 123
            };

            var factDto = new FactDto
            {
                Id = 1,
                Title = "Test Fact",
                ImageId = 1,
                FactContent = "Some content"
            };

            var factEntity = new FactEntity
            {
                Id = 1,
                Title = "Test Fact",
                ImageId = 1,
                FactContent = "Some content",
                StreetcodeId = 123
            };

            var query = new CreateFactCommand(factCreateDto);

            _mockMapper.Setup(m => m.Map<FactEntity>(factCreateDto)).Returns(factEntity);
            _mockRepository.Setup(r => r.FactRepository.CreateAsync(factEntity)).ReturnsAsync(factEntity);
            _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<FactDto>(factEntity)).Returns(factDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(factCreateDto.Title, result.Value.Title);
            Assert.Equal(factCreateDto.ImageId, result.Value.ImageId);
            Assert.Equal(factCreateDto.FactContent, result.Value.FactContent);
        }


        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenMapperReturnsNull()
        {
            // Arrange
            var factDto = new FactCreateDto { };
            var query = new CreateFactCommand(factDto);

            _mockMapper.Setup(m => m.Map<FactEntity>(query.Fact)).Returns((FactEntity)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal("Cannot convert null to fact", result.Errors[0].Message);
            _mockLogger.Verify(l => l.LogError(query, "Cannot convert null to fact"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenSaveChangesFails()
        {
            // Arrange
            var factDto = new FactCreateDto { };
            var query = new CreateFactCommand(factDto);
            var factEntity = new FactEntity { ImageId = 1 };

            _mockMapper.Setup(m => m.Map<FactEntity>(query.Fact)).Returns(factEntity);
            _mockRepository.Setup(r => r.FactRepository.CreateAsync(factEntity)).ReturnsAsync(factEntity);
            _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal("Failed to create a fact", result.Errors[0].Message);
            _mockLogger.Verify(l => l.LogError(query, "Failed to create a fact"), Times.Once);
        }
    }
}
