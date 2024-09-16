using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Moq;
using Xunit;
using MediatR;
using AutoMapper;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;

// Алиас для FactEntity
using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;


namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Fact
{
    public class UpdateFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly UpdateFactHandler _handler;

        public UpdateFactHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new UpdateFactHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_SuccessfulUpdate_ReturnsUpdatedFact()
        {
            // Arrange
            var factUpdateDto = new FactUpdateDto
            {
                Id = 1,
            };
            var command = new UpdateFactCommand(factUpdateDto);

            var factEntity = new FactEntity
            {
                Id = 1,
            };

            var responseDto = new FactUpdateDto
            {
                Id = 1,
            };

            _mapperMock.Setup(mapper => mapper.Map<FactEntity>(factUpdateDto))
                .Returns(factEntity);

            _mapperMock.Setup(mapper => mapper.Map<FactUpdateDto>(factEntity))
                .Returns(responseDto);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.Update(factEntity));

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(responseDto.Id, result.Value.Id);

            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Update(factEntity), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_MappingFails_ReturnsFailure()
        {
            // Arrange
            var factUpdateDto = new FactUpdateDto
            {
                Id = 1,
            };
            var command = new UpdateFactCommand(factUpdateDto);

            _mapperMock.Setup(mapper => mapper.Map<FactEntity>(factUpdateDto))
                .Returns((FactEntity)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot convert null to fact", result.Errors[0].Message);

            _loggerMock.Verify(logger => logger.LogError(command, "Cannot convert null to fact"), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Update(It.IsAny<FactEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SaveChangesFails_ReturnsFailure()
        {
            // Arrange
            var factUpdateDto = new FactUpdateDto
            {
                Id = 1,
            };

            var command = new UpdateFactCommand(factUpdateDto);

            var factEntity = new FactEntity
            {
                Id = 1,
            };

            var responseDto = new FactUpdateDto
            {
                Id = 1,
            };

            _mapperMock.Setup(mapper => mapper.Map<FactEntity>(factUpdateDto))
                .Returns(factEntity);

            _mapperMock.Setup(mapper => mapper.Map<FactUpdateDto>(factEntity))
                .Returns(responseDto);

            _repositoryWrapperMock.Setup(repo => repo.FactRepository.Update(factEntity));

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to update fact", result.Errors[0].Message);

            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Update(factEntity), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(logger => logger.LogError(command, "Failed to update fact"), Times.Once);
        }

    }
}
