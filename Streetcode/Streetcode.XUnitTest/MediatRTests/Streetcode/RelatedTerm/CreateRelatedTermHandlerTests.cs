using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.RelatedTerm;

using RelatedTerm = DAL.Entities.Streetcode.TextContent.RelatedTerm;
using Term = DAL.Entities.Streetcode.TextContent.Term;

public class CreateRelatedTermHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRepositoryWrapper> _repositoryMock;
    private readonly CreateRelatedTermHandler _handler;

    public CreateRelatedTermHandlerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IRepositoryWrapper>();
        _handler = new CreateRelatedTermHandler(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ThrowsCustomException_WhenRelatedTermIsNotMapped()
    {
        // Arrange
        var relatedTermCreatDto = new RelatedTermCreateDto("word", 1);
        var command = new CreateRelatedTermCommand(relatedTermCreatDto);
        
        RepositoryMockSetup(existing: new RelatedTerm(), created: null!, savedChanges: 0, term: null!);
        MapperMockSetup(relatedTerm: null!, relatedTermFullDto: null!);

        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Error while mapping RelatedTerm to RelatedTermFullDto", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.CreateAsync(It.IsAny<RelatedTerm>()), Times.Never);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ReturnsRelatedTermWithTerm_WhenRelatedTermExistsAndMapped()
    {
        // Arrange
        var relatedTermCreatDto = new RelatedTermCreateDto("word", 1);
        var command = new CreateRelatedTermCommand(relatedTermCreatDto);
        var relatedTermFullDto = new RelatedTermFullDto(1, "word", new TermDto());
        
        RepositoryMockSetup(existing: new RelatedTerm(), created: null!, savedChanges: 0, term: null!);
        MapperMockSetup(relatedTerm: null!, relatedTermFullDto: relatedTermFullDto);
    
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(relatedTermFullDto);
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.CreateAsync(It.IsAny<RelatedTerm>()), Times.Never);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ThrowsCustomException_WhenRelatedTermNotExistsAndDtoNotMapped()
    {
        // Arrange
        var relatedTermCreatDto = new RelatedTermCreateDto("word", 1);
        var command = new CreateRelatedTermCommand(relatedTermCreatDto);
        
        RepositoryMockSetup(existing: null, created: null!, savedChanges: 0, term: null!);
        MapperMockSetup(relatedTerm: null!, relatedTermFullDto: null!);

        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Error while mapping RelatedTermCreateDto to RelatedTerm", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.CreateAsync(It.IsAny<RelatedTerm>()), Times.Never);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
    
    [Fact]
    public async Task Handle_ThrowsCustomException_WhenChangesNotSaved()
    {
        // Arrange
        var relatedTermCreatDto = new RelatedTermCreateDto("word", 1);
        var command = new CreateRelatedTermCommand(relatedTermCreatDto);
        var relatedTerm = new RelatedTerm();
        
        RepositoryMockSetup(existing: null, created: relatedTerm, savedChanges: 0, term: null!);
        MapperMockSetup(relatedTerm: relatedTerm, relatedTermFullDto: null!);

        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("No changes made", exception.Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.CreateAsync(It.IsAny<RelatedTerm>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ThrowsCustomException_WhenTermIsNotLoadedIntoCreatedRelatedTerm()
    {
        // Arrange
        var relatedTermCreatDto = new RelatedTermCreateDto("word", 1);
        var command = new CreateRelatedTermCommand(relatedTermCreatDto);
        var relatedTerm = new RelatedTerm();
        
        RepositoryMockSetup(existing: null, created: relatedTerm, savedChanges: 1, term: null!);
        MapperMockSetup(relatedTerm: relatedTerm, relatedTermFullDto: null!);
        
        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Related Term not existing", exception.Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.CreateAsync(It.IsAny<RelatedTerm>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ThrowsCustomException_WhenCreatedRelatedTermNotMapped()
    {
        // Arrange
        var relatedTermCreatDto = new RelatedTermCreateDto("word", 1);
        var command = new CreateRelatedTermCommand(relatedTermCreatDto);
        var relatedTerm = new RelatedTerm();
        
        RepositoryMockSetup(existing: null, created: relatedTerm, savedChanges: 1, term: new Term());
        MapperMockSetup(relatedTerm: relatedTerm, relatedTermFullDto: null!);
        
        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Error while mapping RelatedTerm to RelatedTermFullDto", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.CreateAsync(It.IsAny<RelatedTerm>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
    
        [Fact]
    public async Task Handle_ReturnsRelatedTermFullDto_WhenRelatedTermCreatedAndMapped()
    {
        // Arrange
        var relatedTermCreatDto = new RelatedTermCreateDto("word", 1);
        var command = new CreateRelatedTermCommand(relatedTermCreatDto);
        var relatedTerm = new RelatedTerm();
        var relatedTermFullDto = new RelatedTermFullDto(1, "word", new TermDto());
        
        RepositoryMockSetup(existing: null, created: relatedTerm, savedChanges: 1, term: new Term());
        MapperMockSetup(relatedTerm: relatedTerm, relatedTermFullDto: relatedTermFullDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(relatedTermFullDto);
        _repositoryMock.Verify(repo => repo.RelatedTermRepository.CreateAsync(It.IsAny<RelatedTerm>()), Times.Once);
        _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    private void RepositoryMockSetup(RelatedTerm? existing, RelatedTerm created, int savedChanges, Term term)
    {
        _repositoryMock
            .Setup(repo => repo.RelatedTermRepository
                .GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<RelatedTerm, bool>>>(),
                    It.IsAny<Func<IQueryable<RelatedTerm>, IIncludableQueryable<RelatedTerm, object>>>()))
            .ReturnsAsync(existing);
        _repositoryMock
            .Setup(repo => repo.RelatedTermRepository.CreateAsync(It.IsAny<RelatedTerm>()))
            .ReturnsAsync(created);
        _repositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(savedChanges);
        _repositoryMock
            .Setup(repo => repo.TermRepository
                .GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Term, bool>>>(),
                    It.IsAny<Func<IQueryable<Term>, IIncludableQueryable<Term, object>>>()))
            .ReturnsAsync(term);
    }

    private void MapperMockSetup(RelatedTerm relatedTerm, RelatedTermFullDto relatedTermFullDto)
    {
        _mapperMock
            .Setup(mapper => mapper.Map<RelatedTerm>(It.IsAny<RelatedTermCreateDto>()))
            .Returns(relatedTerm);
        _mapperMock
            .Setup(mapper => mapper.Map<RelatedTermFullDto>(It.IsAny<RelatedTerm?>()))
            .Returns(relatedTermFullDto);
    }
}