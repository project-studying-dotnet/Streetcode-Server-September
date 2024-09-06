using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Text;
using Streetcode.BLL.MediatR.Streetcode.Text.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;


namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Texts.GetByStreetcodeId
{
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

    }
}
