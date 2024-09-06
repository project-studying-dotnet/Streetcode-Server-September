namespace Term.GetById;

using Xunit;
using Moq;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.MediatR.Streetcode.Term.GetById;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using System.Linq.Expressions;
using FluentAssertions;

public class GetTermByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetTermByIdHandler _handler;

    public GetTermByIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetTermByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_TermNotFound_ShouldReturnFailResult()
    {
        // Arrange
        var request = new GetTermByIdQuery(1);
        _repositoryWrapperMock.Setup(repo => repo.TermRepository
            .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Term, bool>>>(), null))
            .ReturnsAsync((Term)null);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message == $"Cannot find any term with corresponding id: {request.Id}");
        _loggerMock.Verify(logger => logger.LogError(request, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TermFound_ShouldReturnOkResult()
    {
        // Arrange
        var request = new GetTermByIdQuery(1);
        var term = new Term { Id = 1, Title = "Test Term" };
        var termDto = new TermDTO { Id = 1, Title = "Test Term" };

        _repositoryWrapperMock.Setup(repo => repo.TermRepository.GetFirstOrDefaultAsync(
            It.Is<Expression<Func<Term, bool>>>(exp => exp.Compile().Invoke(term)),
            null)).ReturnsAsync(term);


        _mapperMock.Setup(m => m.Map<TermDTO>(term)).Returns(termDto);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(termDto, result.ValueOrDefault);

        // Check the call by ID
        _repositoryWrapperMock.Verify(repo => repo.TermRepository.GetFirstOrDefaultAsync(
            It.Is<Expression<Func<Term, bool>>>(exp => exp.Compile().Invoke(new Term { Id = request.Id })),
            null), Times.Once);

        _mapperMock.Verify(m => m.Map<TermDTO>(term), Times.Once);
    }
}