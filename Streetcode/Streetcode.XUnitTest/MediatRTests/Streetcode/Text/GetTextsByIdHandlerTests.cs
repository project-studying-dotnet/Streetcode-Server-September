using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.Extensions;
using Streetcode.BLL.Resources;
using Xunit;

using TextEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Text;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Text
{
    public class GetTextsByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetTextByIdHandler _handler;
        private readonly Mock<IStringLocalizer<ErrorMessages>> _stringLocalizerMock;

        public GetTextsByIdHandlerTests()
        {
            _mockRepository = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _stringLocalizerMock = new Mock<IStringLocalizer<ErrorMessages>>();
            _handler = new GetTextByIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object,
                _stringLocalizerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenTextExists()
        {
            // Arrange
            var text = new TextEntity { Id = 1, TextContent = "Sample text" };
            var textDto = new TextDto { Id = 1, TextContent = "Sample text" };
            var query = new GetTextByIdQuery(1);


            _mockRepository.Setup(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<TextEntity, bool>>>(exp => exp.Compile().Invoke(text)),
                null)).ReturnsAsync(text);


            _mockMapper.Setup(m => m.Map<TextDto>(text)).Returns(textDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(textDto, result.ValueOrDefault);

            // Check the call by ID
            _mockRepository.Verify(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<TextEntity, bool>>>(exp =>
                    exp.Compile().Invoke(new TextEntity { Id = query.Id })),
                null), Times.Once);

            _mockMapper.Verify(m => m.Map<TextDto>(text), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailResult_WhenTextDoesNotExist()
        {
            // Arrange
            var query = new GetTextByIdQuery(1);
            string errorMsg = $"No Text with {query.Id} was found";

            _mockRepository.Setup(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<TextEntity, bool>>>(exp =>
                    exp.Compile().Invoke(new TextEntity { Id = query.Id })),
                null)).ReturnsAsync((TextEntity)null!);

            _stringLocalizerMock
                .Setup(s => s[ErrorKeys.NotFoundError])
                .Returns(new LocalizedString(ErrorKeys.NotFoundError, errorMsg));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(errorMsg, result.Errors.First().Message);

            _mockRepository.Verify(repo => repo.TextRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<TextEntity, bool>>>(exp =>
                    exp.Compile().Invoke(new TextEntity { Id = query.Id })),
                null), Times.Once);

            _mockLogger.Verify(logger => logger.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }
    }
}


