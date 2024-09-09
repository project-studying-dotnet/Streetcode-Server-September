using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Fact 
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
            var facts = new List<FactEntity> { new FactEntity { Id = 1, Title = "Fact Title", FactContent = "Fact Content" } };
            var factsDto = new List<FactDto>() { new FactDto { Id = 1, Title = "Fact Title", FactContent = "Fact Content" } };

            _repositoryWrapperMock.Setup(repo => repo.FactRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<FactEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()))
                .ReturnsAsync(facts);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<FactDto>>(It.IsAny<IEnumerable<FactEntity>>()))
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
                   It.IsAny<Expression<Func<FactEntity, bool>>>(),
                   It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()))
               .ReturnsAsync((IEnumerable<FactEntity>)null);

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

