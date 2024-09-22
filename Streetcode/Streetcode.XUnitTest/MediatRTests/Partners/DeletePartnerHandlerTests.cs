using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.Delete;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Partners
{
    public class DeletePartnerHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeletePartnerHandler _handler;

        public DeletePartnerHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new DeletePartnerHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenPartnerIsDeletedSuccessfully()
        {
            // Arrange
            var testPartner = new Partner { Id = 2, Title = "Test Partner" };
            var testPartnerDto = new PartnerDto { Id = 4, Title = "Test Partner" };

            var command = new DeletePartnerQuery(testPartner.Id);

            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Partner, bool>>>(),
            It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>())).ReturnsAsync(testPartner);

            _mapperMock.Setup(m => m.Map<PartnerDto>(It.Is<Partner>(p => p.Id == testPartner.Id && p.Title == testPartner.Title)))
            .Returns(testPartnerDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(testPartnerDto);
            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.Delete(It.Is<Partner>(p => p.Id == testPartner.Id && p.Title == testPartner.Title)), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<PartnerDto>(It.Is<Partner>(p => p.Id == testPartner.Id && p.Title == testPartner.Title)), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsFailResult_WhenPartnerNotFound()
        {
            // Arrange
            var command = new DeletePartnerQuery(1);

            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Partner, bool>>>(),
            It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>())).ReturnsAsync((Partner)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Be("No partner with such id");
            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.Delete(It.IsAny<Partner>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), "No partner with such id"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailResult_WhenSaveChangesThrowsException()
        {
            // Arrange
            var testPartner = new Partner { Id = 1, Title = "Test Partner" };
            var command = new DeletePartnerQuery(testPartner.Id);
            var exceptionMessage = "Test exception on save";

            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.GetFirstOrDefaultAsync(
            It.IsAny<Expression<Func<Partner, bool>>>(), 
            It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>())).ReturnsAsync(testPartner);
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Be(exceptionMessage);
            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.Delete(testPartner), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), exceptionMessage), Times.Once);
        }

    }
}
