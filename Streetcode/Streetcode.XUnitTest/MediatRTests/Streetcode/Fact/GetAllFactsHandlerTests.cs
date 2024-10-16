﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Exceptions.CustomExceptions;
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
        private readonly GetAllFactsHandler _handler;

        public GetAllFactsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetAllFactsHandler(_repositoryWrapperMock.Object, _mapperMock.Object);
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
        public async Task Handle_ShouldThrowCustomException_WhenFactsNotFound()
        {
            // Arrange
            string expectedErrorMsg = "Cannot find any fact";

            // Настройка мока для метода GetAllAsync без параметров
            _repositoryWrapperMock.Setup(repo => repo.FactRepository
               .GetAllAsync(
                   It.IsAny<Expression<Func<FactEntity, bool>>>(),
                   It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()))!
               .ReturnsAsync((IEnumerable<FactEntity>)null);

            var query = new GetAllFactsQuery();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(query, CancellationToken.None));

            // Проверка сообщения и кода статуса исключения
            Assert.Equal(expectedErrorMsg, exception.Message);
            Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);

            _repositoryWrapperMock.Verify(repo => repo.FactRepository
               .GetAllAsync(
                   It.IsAny<Expression<Func<FactEntity, bool>>>(),
                   It.IsAny<Func<IQueryable<FactEntity>, IIncludableQueryable<FactEntity, object>>>()),
                Times.Once);
        }
    }
}

