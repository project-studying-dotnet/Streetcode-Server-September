using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.Dto.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TransactLink = Streetcode.DAL.Entities.Transactions.TransactionLink;

namespace Streetcode.XUnitTest.MediatRTests.Transactions.TransactionLink
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
            var streetCodeId = 1;
            var transactionLink = new TransactLink { Id = 1, StreetcodeId = streetCodeId };
            var transactionDto = new TransactLinkDto { Id = transactionLink.Id, StreetcodeId = transactionLink.StreetcodeId };
            var query = new GetTransactLinkByStreetcodeIdQuery(streetCodeId);

            _mockRepository.Setup(repo => repo.TransactLinksRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<TransactLink, bool>>>(exp => exp.Compile().Invoke(transactionLink)), null))
            .ReturnsAsync(transactionLink);

            _mockMapper.Setup(m => m.Map<TransactLinkDto>(transactionLink)).Returns(transactionDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ValueOrDefault.Should().BeEquivalentTo(transactionDto);

            _mockRepository.Verify(repo => repo.TransactLinksRepository.GetFirstOrDefaultAsync(
                    It.Is<Expression<Func<TransactLink, bool>>>(
                         exp => exp.Compile().Invoke(new TransactLink { StreetcodeId = query.StreetcodeId })),
            null), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnFailResult_WhenTransactLinkIsNull()
        {
            var streetCodeId = 1;
            var query = new GetTransactLinkByStreetcodeIdQuery(streetCodeId);

            // Arrange
            _mockRepository.Setup(repo => repo.TransactLinksRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TransactLink, bool>>>(), null))
            .ReturnsAsync((TransactLink)null);

            _mockRepository.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(), null))
            .ReturnsAsync((StreetcodeContent)null);

            int id = It.IsAny<int>();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == $"Cannot find a transaction link by a streetcode id: {streetCodeId}, because such streetcode doesn`t exist");
            _mockLogger.Verify(l => l.LogError(It.IsAny<object>(), $"Cannot find a transaction link by a streetcode id: {streetCodeId}, because such streetcode doesn`t exist"), Times.Once);
        }
    }
}
