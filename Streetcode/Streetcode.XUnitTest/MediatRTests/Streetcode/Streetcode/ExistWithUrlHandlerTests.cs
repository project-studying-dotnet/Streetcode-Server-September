using AutoMapper;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.ExistWithUrl;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using StreetcodeEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Streetcode
{
    public class ExistWithUrlHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly ExistWithUrlHandler _handler;

        public ExistWithUrlHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new ExistWithUrlHandler(_repositoryWrapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingUrl_ReturnsTrue()
        {
            // Arrange
            var url = "existing-url";
            var query = new ExistWithUrlQuery(url);
            var streetcode = new StreetcodeEntity { TransliterationUrl = "existing-url" };

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<StreetcodeEntity, bool>>>(exp => exp.Compile().Invoke(streetcode)), null))
                .ReturnsAsync(streetcode);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task Handle_NonExistingUrl_ReturnsFalseAndLogsError()
        {
            // Arrange
            var url = "non-existing-url";
            var query = new ExistWithUrlQuery(url);
            var expectedErrorMsg = $"No streetcode with url: {url}";

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(), null))
                .ReturnsAsync((StreetcodeEntity)null!);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedErrorMsg, result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(query, expectedErrorMsg), Times.Once);
        }
    }
}