using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetAll;
using Streetcode.BLL.Services.Cache;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Text
{
    public class GetAllTextsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly GetAllTextsHandler _handler;
    
        public GetAllTextsHandlerTests()
        {
            _mockRepository = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _mockCacheService = new Mock<ICacheService>();
            _handler = new GetAllTextsHandler(
                _mockRepository.Object, 
                _mockMapper.Object, 
                _mockLogger.Object, 
                _mockCacheService.Object);
        }
    
        [Fact]
        public async Task Handle_ReturnOkResult_WhenTextsExist()
        {
            // Arrange
            var texts = new List<TextEntity> { new TextEntity { Id = 1, TextContent = "Some sample text" } };
            
            _mockCacheService
                .Setup(cache => cache.GetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<IEnumerable<TextEntity>>>>(),
                    It.IsAny<CancellationToken>(),
                    null,
                    null))
                .ReturnsAsync(texts);
    
    
            var textDto = new List<TextDto> { new TextDto { Id = 1, TextContent = "Some sample text" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<TextDto>>(texts)).Returns(textDto);
    
            // Act
            var result = await _handler.Handle(new GetAllTextsQuery(), CancellationToken.None);
    
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(textDto, result.ValueOrDefault);
            _mockMapper.Verify(m => m.Map<IEnumerable<TextDto>>(It.IsAny<IEnumerable<TextEntity>>()), Times.Once);
        }
        
        [Fact]
        public async Task Handle_ReturnFailureResult_WhenTextsDoesNotExist()
        {
            // Arrange
            var texts = Enumerable.Empty<TextEntity>();
            var query = new GetAllTextsQuery();
            _mockCacheService
                .Setup(cache => cache.GetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<IEnumerable<TextEntity>>>>(),
                    It.IsAny<CancellationToken>(),
                    null,
                    null))
                .ReturnsAsync(texts);
    
    
            var textDto = new List<TextDto> { new TextDto { Id = 1, TextContent = "Some sample text" } };
    
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);
    
            // Assert
            Assert.True(result.IsFailed);
            _mockLogger.Verify(l => l.LogError(query, "Cannot find any text"), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<TextDto>>(It.IsAny<IEnumerable<TextEntity>>()), Times.Never);
        }
    }
}
