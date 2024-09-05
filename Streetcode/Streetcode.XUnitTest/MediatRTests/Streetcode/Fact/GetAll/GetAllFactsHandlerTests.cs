using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Org.BouncyCastle.Asn1.Ocsp;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetAll;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Facts.GetAll
{
    public class GetAllFactsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllFactsHandler _handler;

        public GetAllFactsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllFactsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsFactsDto_WhenFactsExist()
        {
            // Arrange
            var facts = new List<Fact> { new Fact { Id = 1, Title = "Fact Title", FactContent = "Fact Content" } };
            var factsDto = new List<FactDto>() { new FactDto { Id = 1, Title = "Fact Title", FactContent = "Fact Content" } };

            _repositoryWrapperMock.Setup(repo => repo.FactRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<Fact, bool>>>(),
                    It.IsAny<Func<IQueryable<Fact>, IIncludableQueryable<Fact, object>>>()))
                .ReturnsAsync(facts);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<FactDto>>(It.IsAny<IEnumerable<Fact>>()))
                .Returns(factsDto);

            // Act
            var result = await _handler.Handle(new GetAllFactsQuery(), CancellationToken.None);

            // Assert
            Assert.Multiple(
                () => Assert.True(result.IsSuccess),
                () => Assert.Equal(factsDto, result.Value));
        }

        [Fact]
        public async Task Handle_ReturnsErrorMsg_WhenFactsNotFound()
        {
            // Arrange
            string expectedErrorMsg = $"Cannot find any fact";

            _repositoryWrapperMock.Setup(repo => repo.FactRepository
               .GetAllAsync(
                   It.IsAny<Expression<Func<Fact, bool>>>(),
                   It.IsAny<Func<IQueryable<Fact>, IIncludableQueryable<Fact, object>>>()))
               .ReturnsAsync((IEnumerable<Fact>)null);

            // Act
            var result = await _handler.Handle(new GetAllFactsQuery(), CancellationToken.None);

            // Assert
            Assert.Multiple(
                () => Assert.True(result.IsFailed),
                () => Assert.Equal(expectedErrorMsg, result.Errors.First().Message));
            _loggerMock.Verify(x => x.LogError(It.IsAny<object>(), It.Is<string>(s => s.Contains(expectedErrorMsg))), Times.Once);
        }
    }
}
