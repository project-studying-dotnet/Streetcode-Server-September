using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.DeleteHard;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.Tests.UnitTest.Streetcode.Streetcode.DeleteHard
{
    public class DeleteHardStreetcodeHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly DeleteHardStreetcodeHandler _handler;

        public DeleteHardStreetcodeHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _handler = new DeleteHardStreetcodeHandler(_repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task Handle_StreetcodeExistsAndDeletionSucceeds_ReturnsSuccess()
        {
            // Arrange
            int streetcodeId = 1;
            var streetcode = new StreetcodeContent { Id = streetcodeId };

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ReturnsAsync(streetcode);

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.Delete(streetcode));

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(1);

            var request = new DeleteHardStreetcodeCommand(streetcodeId);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_StreetcodeDoesNotExist_ThrowsCustomException()
        {
            // Arrange
            int streetcodeId = 1;

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ReturnsAsync((StreetcodeContent)null);

            var request = new DeleteHardStreetcodeCommand(streetcodeId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(request, CancellationToken.None));

            Assert.Equal($"No streetcode found with Id - {streetcodeId}", exception.Message);
            Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);
        }

        [Fact]
        public async Task Handle_SaveChangesFails_ThrowsCustomException()
        {
            // Arrange
            int streetcodeId = 1;
            var streetcode = new StreetcodeContent { Id = streetcodeId };

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
                .ReturnsAsync(streetcode);

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.Delete(streetcode));

            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync())
                .ReturnsAsync(0);

            var request = new DeleteHardStreetcodeCommand(streetcodeId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(request, CancellationToken.None));

            Assert.Equal("Failed to delete streetcode", exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }
    }
}
