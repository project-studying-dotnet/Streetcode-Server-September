﻿using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Transactions;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using TransactLink = Streetcode.DAL.Entities.Transactions.TransactionLink;

namespace Streetcode.XUnitTest.MediatRTests.Transactions.TransactionLink
{
    public class GetTransactLinkByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetTransactLinkByIdHandler _handler;

        public GetTransactLinkByIdHandlerTests()
        {
            _mockRepository = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new GetTransactLinkByIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenTransactLinkExist()
        {
            // Arrange
            var id = 1;
            var transactionLink = new TransactLink { Id = id, StreetcodeId = 1 };
            var transactionDto = new TransactLinkDto { Id = transactionLink.Id, StreetcodeId = transactionLink.StreetcodeId };
            var query = new GetTransactLinkByIdQuery(id);

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
                         exp => exp.Compile().Invoke(new TransactLink { Id = query.Id })),
            null), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnFailResult_WhenTransactLinkIsNull()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.TransactLinksRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<TransactLink, bool>>>(),
                It.IsAny<Func<IQueryable<TransactLink>, IIncludableQueryable<TransactLink, object>>>()
                ))
            .ReturnsAsync((TransactLink)null!);

            int id = It.IsAny<int>();

            // Act
            var result = await _handler.Handle(new GetTransactLinkByIdQuery(id), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == $"Cannot find any transaction link with corresponding id: {id}");
            _mockLogger.Verify(l => l.LogError(It.IsAny<object>(), $"Cannot find any transaction link with corresponding id: {id}"), Times.Once);
        }
    }
}
