using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetById;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace Texts.GetById
{
    public class GetTextsByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetTextByIdHandler _handler;

        public GetTextsByIdHandlerTests()
        {
            _mockRepository = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new GetTextByIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenTextExists()
        {
            // Arrange
            var text = new Text { Id = 1, TextContent = "Sample text" };
            var textDto = new TextDTO { Id = 1, TextContent = "Sample text" };
            _mockRepository.Setup(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Text, bool>>>(),
                null)).ReturnsAsync(text);

            _mockMapper.Setup(m => m.Map<TextDTO>(text)).Returns(textDto);

            var query = new GetTextByIdQuery(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(textDto, result.ValueOrDefault);

            _mockRepository.Verify(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Text, bool>>>(),
                null), Times.Once);

            _mockMapper.Verify(m => m.Map<TextDTO>(text), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailResult_WhenTextDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Text, bool>>>(),
                null)).ReturnsAsync((Text)null);

            var query = new GetTextByIdQuery(1);
            string errorMsg = $"Cannot find any text with corresponding id: {query.Id}";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMsg, result.Errors.First().Message);

            _mockRepository.Verify(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Text, bool>>>(),
                null), Times.Once);

            _mockLogger.Verify(logger => logger.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }
    }
}
