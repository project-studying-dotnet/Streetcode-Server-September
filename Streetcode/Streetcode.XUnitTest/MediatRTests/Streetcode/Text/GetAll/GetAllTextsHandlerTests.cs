using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetAll;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;


namespace Streetcode.XUnitTest
{
    public class GetAllTextsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetAllTextsHandler _handler;

        public GetAllTextsHandlerTests()
        {
            _mockRepository = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new GetAllTextsHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
        }


        [Fact]
        public async Task Handle_ReturnOkResult_WhenTextsExist()
        {
            // Arrange
            var texts = new List<Text> { new Text { Id = 1, TextContent = "Some sample text" } };
            var mockTextRepository = new Mock<ITextRepository>();
            mockTextRepository.Setup(repo => repo.GetAllAsync(
                It.IsAny<Expression<Func<Text, bool>>>(),
                It.IsAny<Func<IQueryable<Text>, IIncludableQueryable<Text, object>>>()
            )).ReturnsAsync(texts);

            _mockRepository.Setup(repo => repo.TextRepository).Returns(mockTextRepository.Object);

            var textDto = new List<TextDTO> { new TextDTO { Id = 1, TextContent = "Some sample text" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<TextDTO>>(texts)).Returns(textDto);

            // Act
            var result = await _handler.Handle(new GetAllTextsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(textDto, result.ValueOrDefault);

            mockTextRepository.Verify(repo => repo.GetAllAsync(
                It.IsAny<Expression<Func<Text, bool>>>(),
                It.IsAny<Func<IQueryable<Text>, IIncludableQueryable<Text, object>>>()
            ), Times.Once);

            _mockMapper.Verify(m => m.Map<IEnumerable<TextDTO>>(It.IsAny<IEnumerable<Text>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailResult_WhenTextsAreNull()
        {
            // Arrange
            var mockTextRepository = new Mock<ITextRepository>();

            mockTextRepository.Setup(repo => repo.GetAllAsync(
                It.IsAny<Expression<Func<Text, bool>>>(),
                It.IsAny<Func<IQueryable<Text>, IIncludableQueryable<Text, object>>>()
            )).ReturnsAsync((IEnumerable<Text>)null);

            _mockRepository.Setup(repo => repo.TextRepository).Returns(mockTextRepository.Object);

            const string errorMsg = "Cannot find any text";

            // Act
            var result = await _handler.Handle(new GetAllTextsQuery(), CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMsg, result.Errors.First().Message);
            mockTextRepository.Verify(repo => repo.GetAllAsync(
                It.IsAny<Expression<Func<Text, bool>>>(),
                It.IsAny<Func<IQueryable<Text>, IIncludableQueryable<Text, object>>>()
            ), Times.Once);
            _mockLogger.Verify(logger => logger.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }
    }
}
