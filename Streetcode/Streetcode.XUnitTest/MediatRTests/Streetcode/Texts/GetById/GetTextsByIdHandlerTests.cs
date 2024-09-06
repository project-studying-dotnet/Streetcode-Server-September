namespace Texts.GetById;
using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Text.GetById;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

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
    var textDto = new TextDto { Id = 1, TextContent = "Sample text" };
    var query = new GetTextByIdQuery(1);


    _mockRepository.Setup(repo => repo.TextRepository.GetFirstOrDefaultAsync(
    It.Is<Expression<Func<Text, bool>>>(exp => exp.Compile().Invoke(text)),
    null)).ReturnsAsync(text);


    _mockMapper.Setup(m => m.Map<TextDto>(text)).Returns(textDto);

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(textDto, result.ValueOrDefault);

    // Check the call by ID
    _mockRepository.Verify(repo => repo.TextRepository.GetFirstOrDefaultAsync(
        It.Is<Expression<Func<Text, bool>>>(exp => exp.Compile().Invoke(new Text { Id = query.Id })),
        null), Times.Once);

    _mockMapper.Verify(m => m.Map<TextDto>(text), Times.Once);
}

[Fact]
public async Task Handle_ReturnsFailResult_WhenTextDoesNotExist()
{
    // Arrange
    var query = new GetTextByIdQuery(1);
    string errorMsg = $"Cannot find any text with corresponding id: {query.Id}";

    _mockRepository.Setup(repo => repo.TextRepository.GetFirstOrDefaultAsync(
        It.Is<Expression<Func<Text, bool>>>(exp => exp.Compile().Invoke(new Text { Id = query.Id })),
        null)).ReturnsAsync((Text)null);

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(errorMsg, result.Errors.First().Message);

    _mockRepository.Verify(repo => repo.TextRepository.GetFirstOrDefaultAsync(
        It.Is<Expression<Func<Text, bool>>>(exp => exp.Compile().Invoke(new Text { Id = query.Id })),
        null), Times.Once);

    _mockLogger.Verify(logger => logger.LogError(It.IsAny<object>(), errorMsg), Times.Once);
}
}
