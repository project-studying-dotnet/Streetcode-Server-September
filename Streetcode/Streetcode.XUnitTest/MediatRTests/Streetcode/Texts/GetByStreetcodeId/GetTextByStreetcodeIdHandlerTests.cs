namespace Texts.GetByStreetcodeId;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Text;
using Streetcode.BLL.MediatR.Streetcode.Text.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

public class GetTextByStreetcodeIdHandlerTests
{

private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
private readonly Mock<IMapper> _mapperMock;
private readonly Mock<ITextService> _textServiceMock;
private readonly Mock<ILoggerService> _loggerMock;
private readonly GetTextByStreetcodeIdHandler _handler;

public GetTextByStreetcodeIdHandlerTests()
{
    _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
    _mapperMock = new Mock<IMapper>();
    _textServiceMock = new Mock<ITextService>();
    _loggerMock = new Mock<ILoggerService>();

    _handler = new GetTextByStreetcodeIdHandler(
        _repositoryWrapperMock.Object,
        _mapperMock.Object,
        _textServiceMock.Object,
        _loggerMock.Object
    );
}

[Fact]
public async Task Handle_ShouldReturnText_WhenTextExists()
{
    // Arrange
    int streetcodeId = 1;
    var query = new GetTextByStreetcodeIdQuery(streetcodeId);
    var text = new Text
    {
        StreetcodeId = streetcodeId,
        TextContent = "Sample text",
        Title = "Sample Title",
        AdditionalText = "Additional Info"
    };
    var textWithTags = "Sample text with tags";
    var textDTO = new TextDTO
    {
        StreetcodeId = streetcodeId,
        TextContent = textWithTags,
        Title = "Sample Title",
        AdditionalText = "Additional Info"
    };

    _repositoryWrapperMock.Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
        It.Is<Expression<Func<Text, bool>>>(expr => expr.Compile()(text)),
        null))
    .ReturnsAsync(text);

    _textServiceMock.Setup(t => t.AddTermsTag(text.TextContent)).ReturnsAsync(textWithTags);
    _mapperMock.Setup(m => m.Map<TextDTO?>(It.Is<Text>(t => t.TextContent == textWithTags))).Returns(textDTO);

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Should().BeEquivalentTo(textDTO);

}

[Fact]
public async Task Handle_ShouldReturnSuccessWithNull_WhenTextNotFoundButStreetcodeExists()
{
    // Arrange
    int streetcodeId = 1;
    var query = new GetTextByStreetcodeIdQuery(streetcodeId);

    _repositoryWrapperMock.Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
It.Is<Expression<Func<Text, bool>>>(expr => expr.Compile().Invoke(new Text { StreetcodeId = streetcodeId })), null))
.ReturnsAsync((Text?)null);

    _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
        It.Is<Expression<Func<StreetcodeContent, bool>>>(expr => expr.Compile().Invoke(new StreetcodeContent { Id = streetcodeId })), null))
        .ReturnsAsync(new StreetcodeContent { Id = streetcodeId });

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeNull();
}

[Fact]
public async Task Handle_ShouldLogError_WhenTextAndStreetcodeNotFound()
{
    // Arrange
    int streetcodeId = 1;
    var query = new GetTextByStreetcodeIdQuery(streetcodeId);

    _repositoryWrapperMock.Setup(r => r.TextRepository.GetFirstOrDefaultAsync(
        It.IsAny<Expression<Func<Text, bool>>>(), null))
        .ReturnsAsync((Text?)null);

    _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
        It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
        .ReturnsAsync((StreetcodeContent?)null);

    string expectedErrorMessage = $"Cannot find a transaction link by a streetcode id: {streetcodeId}, because such streetcode doesn`t exist";

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsFailed.Should().BeTrue();
    result.Errors.Should().ContainSingle(e => e.Message == expectedErrorMessage);

    _loggerMock.Verify(l => l.LogError(It.IsAny<GetTextByStreetcodeIdQuery>(), expectedErrorMessage), Times.Once);
}
}
