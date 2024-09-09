using MediatR;
using Moq;
using Streetcode.BLL.Dto.Email;
using Streetcode.BLL.Interfaces.Email;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Email;
using Streetcode.DAL.Entities.AdditionalContent.Email;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Email
{
    public class SendEmailHandlerTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly SendEmailHandler _handler;

        public SendEmailHandlerTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new SendEmailHandler(_mockEmailService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenEmailIsSentSuccessfully()
        {
            // Arrange
            var emailDto = new EmailDto
            {
                From = "from@test.com",
                Content = "Content"
            };

            var command = new SendEmailCommand(emailDto);

            _mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<Message>())).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(Unit.Value, result.Value);

            // Optional: Verify if the email was correctly formatted
            Assert.Equal("from@test.com", command.Email.From);
            Assert.Equal("Content", command.Email.Content);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenEmailSendingFails()
        {
            // Arrange
            var emailDto = new EmailDto
            {
                From = "from@test.com",
                Content = "Content"
            };

            var command = new SendEmailCommand(emailDto);

            _mockEmailService.Setup(s => s.SendEmailAsync(It.IsAny<Message>())).ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to send email message", result.Errors[0].Message);

            _mockLogger.Verify(l => l.LogError(It.IsAny<object>(), "Failed to send email message"), Times.Once);

            // Optional: Verify email properties
            Assert.Equal("from@test.com", command.Email.From);
            Assert.Equal("Content", command.Email.Content);
        }
    }
}
