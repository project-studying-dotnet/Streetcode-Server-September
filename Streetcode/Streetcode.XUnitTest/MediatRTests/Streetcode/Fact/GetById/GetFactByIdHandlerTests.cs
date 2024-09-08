namespace Fact.GetById;

using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
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

public class GetFactByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetFactByIdHandler _handler;

    public GetFactByIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetFactByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsError_WhenFactNotExist()
    {
        // Arrange
        var factId = 1;
        var request = new GetFactByIdQuery(factId);
        string errorMsg = $"Cannot find any fact with corresponding id: {factId}";
        _repositoryWrapperMock.Setup(repo => repo.FactRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Fact, bool>>>(), null))
            .ReturnsAsync((Fact)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Multiple(
            () => Assert.True(result.IsFailed));
        Assert.Equal(errorMsg, result.Errors.First().Message);
        _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.FactRepository.GetFirstOrDefaultAsync(f => f.Id == factId, null), Times.Once);
        _mapperMock.Verify(m => m.Map<FactDto>(It.IsAny<Fact>()), Times.Never);

    }

    [Fact]
    public async Task Handle_ReturnOkResult_WhenFactFound() 
    {
        var factId = 1;
        var request = new GetFactByIdQuery(factId);
        var fact = new Fact { Id = factId, Title = "Test Fact" };
        var factDto = new FactDto { Id = factId, Title = "Test Fact" };

        _repositoryWrapperMock.Setup(repo => repo.FactRepository
            .GetFirstOrDefaultAsync(f => f.Id == factId, null))
            .ReturnsAsync(fact);

        _mapperMock.Setup(m => m.Map<FactDto>(fact)).Returns(factDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(factDto, result.Value);
        _repositoryWrapperMock.Verify(repo => repo.FactRepository.GetFirstOrDefaultAsync(f => f.Id == factId, null), Times.Once);
        _mapperMock.Verify(m => m.Map<FactDto>(fact), Times.Once);
        _loggerMock.VerifyNoOtherCalls();
    }
}
