﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetById;
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
    public class GetFactByIdHandlerTest
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetFactByIdHandler _handler;

        public GetFactByIdHandlerTest()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetFactByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldThrowCustomException_WhenFactNotExist()
        {
            // Arrange
            var factId = 1;
            var request = new GetFactByIdQuery(factId);
            string errorMsg = $"Cannot find any fact with corresponding id: {factId}";

            _repositoryWrapperMock.Setup(repo => repo.FactRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<FactEntity, bool>>>(), null))
                .ReturnsAsync((FactEntity)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(request, CancellationToken.None));

            Assert.Equal($"Cannot find any fact with corresponding id: {request.Id}", exception.Message);
            Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);

            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenFactFound()
        {
            var factId = 1;
            var request = new GetFactByIdQuery(factId);
            var fact = new FactEntity { Id = factId, Title = "Test Fact" };
            var factDto = new FactDto { Id = factId, Title = "Test Fact" };

            _repositoryWrapperMock.Setup(repo => repo.FactRepository
                .GetFirstOrDefaultAsync(It.Is<Expression<Func<FactEntity, bool>>>(exp => exp.Compile().Invoke(fact)), null))
                .ReturnsAsync(fact);

            _mapperMock.Setup(m => m.Map<FactDto>(fact)).Returns(factDto);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(factDto, result.Value);
            _repositoryWrapperMock.Verify(repo => repo.FactRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<FactEntity, bool>>>(
                    exp => exp.Compile().Invoke(new FactEntity { Id = request.Id })), null), Times.Once);

            _mapperMock.Verify(m => m.Map<FactDto>(fact), Times.Once);
        }
    }
}
