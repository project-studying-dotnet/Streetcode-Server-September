using FluentAssertions;
using Moq;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Xunit;

using RelatedTermEntity = Streetcode.DAL.Entities.Streetcode.TextContent.RelatedTerm;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.RelatedTerm;

public class DeleteRelatedTermHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryMock;
    private readonly DeleteRelatedTermHandler _handler;

    public DeleteRelatedTermHandlerTests()
    {
        _repositoryMock = new Mock<IRepositoryWrapper>();
        _handler = new DeleteRelatedTermHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenDTOFormed()
    {
        // Arrange
        var query = new DeleteRelatedTermCommand(It.IsAny<string>(), It.IsAny<int>());
        var entity = new RelatedTermEntity();

        _repositoryMock
            .Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(), null))
            .ReturnsAsync(entity);
        
        _repositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.Delete(It.IsAny<RelatedTermEntity>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenRelatedTermNotFound()
    {
        // Arrange
        const string Word = "word";
        const int TermId = 1;
        var command = new DeleteRelatedTermCommand(Word, TermId);
    
        _repositoryMock
            .Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(), null))
            .ReturnsAsync((RelatedTermEntity)null!);
    
        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
    
        // Assert
        Assert.Equal($"Cannot find a related term: {Word}, termId = {TermId}", exception.Message);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.Delete(It.IsAny<RelatedTermEntity>()), Times.Never);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ReturnsError_WhenSaveFailed()
    {
        // Arrange
        const string Word = "word";
        const int TermId = 1;
        var command = new DeleteRelatedTermCommand(Word, TermId);
        var entity = new RelatedTermEntity();
    
        _repositoryMock
            .Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(), null))
            .ReturnsAsync(entity);
    
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);
    
        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
    
        // Assert
        Assert.Equal("Failed to delete a related term", exception.Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.Delete(It.IsAny<RelatedTermEntity>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}