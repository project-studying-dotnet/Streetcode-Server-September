using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Org.BouncyCastle.Asn1.Ocsp;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetByStreetcodeId;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Fact
{
    public class GetFactByStreetcodeIdHandlerTest
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetFactByStreetcodeIdHandler _handler;

        public GetFactByStreetcodeIdHandlerTest()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetFactByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenFactByStreetcodeIdNotFound()
        {
            var streetcodeId = 1;
            var request = new GetFactByStreetcodeIdQuery(streetcodeId);
            string errorMsg = $"Cannot find any fact by the streetcode id: {streetcodeId}";

            _repositoryWrapperMock.Setup(repo => repo.FactRepository
               .GetAllAsync(
                   It.IsAny<Expression<Func<FactEntity, bool>>>(),
                   It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()))
               .ReturnsAsync((IEnumerable<FactEntity>)null);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(errorMsg, result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenFactByStreetcodeIdExists()
        {
            var streetcodeId = 1;
            var request = new GetFactByStreetcodeIdQuery(streetcodeId);
            var facts = new List<FactEntity>
            {
                new FactEntity { Id = 1, Title = "Test Fact 1", StreetcodeId = streetcodeId },
                new FactEntity { Id = 2, Title = "Test Fact 2", StreetcodeId = 2 },
            };
            var factsDto = new List<FactDto>
            {
                new FactDto { Id = 1, Title = "Test Fact 1" },
            };

            _repositoryWrapperMock.Setup(repo => repo.FactRepository
               .GetAllAsync(
                   It.IsAny<Expression<Func<FactEntity, bool>>>(),
                   It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()))
               .ReturnsAsync(facts);

            _mapperMock.Setup(m => m.Map<IEnumerable<FactDto>>(facts)).Returns(factsDto);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(factsDto, result.Value);
            Assert.Single(result.Value);
            _repositoryWrapperMock.Verify(repo => repo.FactRepository.GetAllAsync(
                   It.Is<Expression<Func<FactEntity, bool>>>(predicate => predicate.Compile().Invoke(facts[0])),
                   default));
        }
    }
}
