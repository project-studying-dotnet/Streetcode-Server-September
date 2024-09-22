using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Streetcode.Term.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Term;

using Term = DAL.Entities.Streetcode.TextContent.Term;

public class CreateTermHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateTermHandler _handler;

    public CreateTermHandlerTests()
    {
         _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
         _mapperMock = new Mock<IMapper>();
         _handler = new CreateTermHandler(_repositoryWrapperMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenDtoIsNotMapped()
    {
        // Arrange
        var termCreateDto = new TermCreateDto("title", "description");
        var command = new CreateTermCommand(termCreateDto);
        _mapperMock.Setup(m => m.Map<Term>(It.IsAny<TermCreateDto>())).Returns((Term)null!);

        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Error while mapping TermCreateDto to Term", exception.Message);
        Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        _repositoryWrapperMock.Verify(r => r.TermRepository.CreateAsync(It.IsAny<Term>()), Times.Never);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenSaveChangesFails()
    {
        // Arrange
        var termCreateDto = new TermCreateDto("title", "description");
        var command = new CreateTermCommand(termCreateDto);
        var term = new Term();
        
        _mapperMock.Setup(m => m.Map<Term>(It.IsAny<TermCreateDto>())).Returns(term);
        _repositoryWrapperMock.Setup(r => r.TermRepository.CreateAsync(It.IsAny<Term>())).ReturnsAsync(term);
        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);
        
        // Act
        var exception =
            await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));
        
        // Assert
        Assert.Equal("Error while creating Term", exception.Message);
        Assert.Equal(StatusCodes.Status500InternalServerError, exception.StatusCode);
        _repositoryWrapperMock.Verify(r => r.TermRepository.CreateAsync(It.IsAny<Term>()), Times.Once);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnTermId_WhenCreationSuccessful()
    {
        // Arrange
        var termCreateDto = new TermCreateDto("title", "description");
        var command = new CreateTermCommand(termCreateDto); 
        var term = new Term { Id = 1 };
        
        _mapperMock.Setup(m => m.Map<Term>(It.IsAny<TermCreateDto>())).Returns(term);
        _repositoryWrapperMock.Setup(r => r.TermRepository.CreateAsync(It.IsAny<Term>())).ReturnsAsync(term);
        _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        _mapperMock.Verify(m => m.Map<Term>(It.IsAny<TermCreateDto>()), Times.Once);
        _repositoryWrapperMock.Verify(r => r.TermRepository.CreateAsync(It.IsAny<Term>()), Times.Once);
        _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}