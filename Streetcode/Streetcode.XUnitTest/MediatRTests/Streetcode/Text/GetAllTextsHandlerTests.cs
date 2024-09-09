using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Text
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
            var texts = new List<TextEntity> { new TextEntity { Id = 1, TextContent = "Some sample text" } };
            _mockRepository.Setup(
            repo => repo.TextRepository.GetAllAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()
            )).ReturnsAsync(texts);


            var textDto = new List<TextDto> { new TextDto { Id = 1, TextContent = "Some sample text" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<TextDto>>(texts)).Returns(textDto);

            // Act
            var result = await _handler.Handle(new GetAllTextsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(textDto, result.ValueOrDefault);

            _mockRepository.Verify(repo => repo.TextRepository.GetAllAsync(
                It.IsAny<Expression<Func<TextEntity, bool>>>(),
                It.IsAny<Func<IQueryable<TextEntity>, IIncludableQueryable<TextEntity, object>>>()
            ), Times.Once);

            _mockMapper.Verify(m => m.Map<IEnumerable<TextDto>>(It.IsAny<IEnumerable<TextEntity>>()), Times.Once);
        }
    }
}
