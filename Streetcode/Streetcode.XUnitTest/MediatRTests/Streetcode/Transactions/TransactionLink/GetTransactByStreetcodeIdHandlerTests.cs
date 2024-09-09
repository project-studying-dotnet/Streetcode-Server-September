using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetById;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using TransactLink = Streetcode.DAL.Entities.Transactions.TransactionLink;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Transactions.TransactionLink
{
    public class GetTransactByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetTransactLinkByStreetcodeIdHandler _handler;

        public GetTransactByStreetcodeIdHandlerTests()
        {
            _mockRepository = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new GetTransactLinkByStreetcodeIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenTransactLinkExist()
        {
            // Arrange
            var transactionLink = new TransactLink { Id = 1, StreetcodeId = 1 };

            _mockRepository.Setup(repo => repo.TransactLinksRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TransactLink, bool>>>(),
                It.IsAny<Func<IQueryable<TransactLink>, IIncludableQueryable<TransactLink, object>>>()
                ))
            .ReturnsAsync(transactionLink);

            var transactionDto = new TransactLinkDto { Id = transactionLink.Id, StreetcodeId = transactionLink.StreetcodeId };
            _mockMapper.Setup(m => m.Map<TransactLinkDto>(transactionLink)).Returns(transactionDto);

            // Act
            var result = await _handler.Handle(new GetTransactLinkByStreetcodeIdQuery(transactionLink.StreetcodeId), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ValueOrDefault.Should().BeEquivalentTo(transactionDto);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenTransactLinkIsNull()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.TransactLinksRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TransactLink, bool>>>(),
                It.IsAny<Func<IQueryable<TransactLink>, IIncludableQueryable<TransactLink, object>>>()
                ))
            .ReturnsAsync((TransactLink)null);

            _mockRepository.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()
                ))
            .ReturnsAsync((StreetcodeContent)null);

            int id = It.IsAny<int>();

            // Act
            var result = await _handler.Handle(new GetTransactLinkByStreetcodeIdQuery(id), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == $"Cannot find a transaction link by a streetcode id: {id}, because such streetcode doesn`t exist");
            _mockLogger.Verify(l => l.LogError(It.IsAny<object>(), $"Cannot find a transaction link by a streetcode id: {id}, because such streetcode doesn`t exist"), Times.Once);
        }
    }
}
