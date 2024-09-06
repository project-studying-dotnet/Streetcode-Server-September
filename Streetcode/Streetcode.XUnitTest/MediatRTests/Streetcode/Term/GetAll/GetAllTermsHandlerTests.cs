namespace Term.GetAll;

using Xunit;
using Moq;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Streetcode.BLL.MediatR.Streetcode.Term.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using System.Collections.Generic;
using MediatR;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

public class GetAllTermsHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly GetAllTermsHandler _handler;

    public GetAllTermsHandlerTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILoggerService>();
        _handler = new GetAllTermsHandler(_mockRepositoryWrapper.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTerms_WhenTermsExist()
    {
        // Arrange
        var terms = new List<Term> { new Term { Id = 1, Title = "Test Term" } };
        _mockRepositoryWrapper.Setup(
            repo => repo.TermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Term, bool>>>(), It.IsAny<Func<IQueryable<Term>, IIncludableQueryable<Term, object>>>()))
        .ReturnsAsync(terms);


        var termDtos = new List<TermDto> { new TermDto { Id = 1, Title = "Test Term" } };
        _mockMapper.Setup(mapper => mapper.Map<IEnumerable<TermDto>>(terms))
            .Returns(termDtos);

        // Act
        var result = await _handler.Handle(new GetAllTermsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(terms.Count, result.Value.Count());
    }

    [Fact]
    public async Task Handle_ReturnsFail_WhenTermsNotFound()
    {
        // Arrange
        _mockRepositoryWrapper.Setup(
            repo => repo.TermRepository.GetAllAsync(
                It.IsAny<Expression<Func<Term, bool>>>(), It.IsAny<Func<IQueryable<Term>, IIncludableQueryable<Term, object>>>()))
            .ReturnsAsync((IEnumerable<Term>)null);

        // Act
        var result = await _handler.Handle(new GetAllTermsQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Cannot find any term", result.Errors.First().Message);
        _mockLogger.Verify(logger => logger.LogError(It.IsAny<GetAllTermsQuery>(), "Cannot find any term"), Times.Once);
    }
}