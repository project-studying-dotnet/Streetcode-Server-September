using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using TransactLink = Streetcode.DAL.Entities.Transactions.TransactionLink;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Transactions.TransactionLink
{
    public class GetAllTransactLinksHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetAllTransactLinksHandler _handler;

        public GetAllTransactLinksHandlerTests()
        {
            _mockRepository = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new GetAllTransactLinksHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenTransactLinksExist()
        {
            // Arrange
            var transactionLink = new TransactLink { Id = 1, StreetcodeId = 1 };

            var transactionLinks = new List<TransactLink>
            {
                transactionLink,
            };

            _mockRepository.Setup(repo => repo.TransactLinksRepository.GetAllAsync(
                It.IsAny<Expression<Func<TransactLink, bool>>>(),
                It.IsAny<Func<IQueryable<TransactLink>, IIncludableQueryable<TransactLink, object>>>()
                ))
            .ReturnsAsync(transactionLinks);

            var transactionDtoList = new List<TransactLinkDto> { new TransactLinkDto { Id = transactionLink.Id, StreetcodeId = transactionLink.StreetcodeId } };
            _mockMapper.Setup(m => m.Map<IEnumerable<TransactLinkDto>>(transactionLinks)).Returns(transactionDtoList);

            // Act
            var result = await _handler.Handle(new GetAllTransactLinksQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ValueOrDefault.Should().BeEquivalentTo(transactionDtoList);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenTransactLinksAreNull()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.TransactLinksRepository.GetAllAsync(
                It.IsAny<Expression<Func<TransactLink, bool>>>(),
                It.IsAny<Func<IQueryable<TransactLink>, IIncludableQueryable<TransactLink, object>>>()
                ))
            .ReturnsAsync((IEnumerable<TransactLink>) null);

            // Act
            var result = await _handler.Handle(new GetAllTransactLinksQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == "Cannot find any transaction link");
            _mockLogger.Verify(l => l.LogError(It.IsAny<object>(), "Cannot find any transaction link"), Times.Once);
        }
    }
}
