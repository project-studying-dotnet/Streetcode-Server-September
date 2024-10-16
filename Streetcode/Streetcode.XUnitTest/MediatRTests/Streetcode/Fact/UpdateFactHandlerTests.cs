using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Moq;
using Xunit;
using MediatR;
using AutoMapper;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;

using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;


namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Fact
{
    public class UpdateFactHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UpdateFactHandler _handler;

        public UpdateFactHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();

            _handler = new UpdateFactHandler(
                _repositoryWrapperMock.Object,
                _mapperMock.Object);
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
        }

        [Fact]
        public async Task Handle_ShouldThrowCustomException_WhenMappingFails()
        {
            // Arrange
            var factUpdateDto = new FactUpdateDto
            {
                Id = 1,
            };
            var command = new UpdateFactCommand(factUpdateDto);

            _mapperMock.Setup(mapper => mapper.Map<FactEntity>(factUpdateDto))
                .Returns((FactEntity)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Cannot convert null to fact", exception.Message);
            Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);

            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Update(It.IsAny<FactEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowCustomException_WhenSaveChangesFails()
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

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Failed to update fact", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);

            _repositoryWrapperMock.Verify(repo => repo.FactRepository.Update(factEntity), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
