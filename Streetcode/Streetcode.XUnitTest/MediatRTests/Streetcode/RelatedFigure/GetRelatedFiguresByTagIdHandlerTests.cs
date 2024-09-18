using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedFigure.GetByTagId;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Dto.Streetcode.RelatedFigure;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.RelatedFigure;

using Xunit;
using AutoMapper;
using Moq;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using RelatedFigure = DAL.Entities.Streetcode.RelatedFigure;

public class GetRelatedFiguresByTagIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetRelatedFiguresByTagIdHandler _handler;
    
    public GetRelatedFiguresByTagIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetRelatedFiguresByTagIdHandler(
            _repositoryWrapperMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenStreetcodesNotFound()
    {
        // Arrange
        const int TagId = 1;
        var query = new GetRelatedFiguresByTagIdQuery(TagId);
    
        _repositoryWrapperMock
            .Setup(r => r.StreetcodeRepository.GetAllAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync((IEnumerable<StreetcodeContent>)null!);
    
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
    
        // Assert
        result.IsFailed.Should().BeTrue();
        _loggerMock.Verify(
            l => l.LogError(query, $"Cannot find any streetcode with corresponding tagid: {query.tagId}"),
            Times.Once);
        _mapperMock.Verify(
            m => m.Map<IEnumerable<RelatedFigure>>(It.IsAny<IEnumerable<StreetcodeContent>>()),
            Times.Never);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenStreetcodesAreFound()
    {
        // Arrange
        var streetcodes = new List<StreetcodeContent>
        {
            new StreetcodeContent
            {
                Id = 1,
                Status = StreetcodeStatus.Published,
                Tags = new List<Tag> { new Tag { Id = 1 }, },
            },
            new StreetcodeContent
            {
                Id = 2,
                Status = StreetcodeStatus.Draft,
                Tags = new List<Tag>
                {
                    new Tag { Id = 1 },
                    new Tag { Id = 2 },
                },
            },
            new StreetcodeContent
            {
                Id = 3,
                Status = StreetcodeStatus.Published,
                Tags = new List<Tag> { new Tag { Id = 3 } },
            }
        };
        
        var relatedFigures = new List<RelatedFigureDto>
        {
            new RelatedFigureDto { Id = 1, },
        };
    
        const int TagId = 1;
        var query = new GetRelatedFiguresByTagIdQuery(TagId);
    
        _repositoryWrapperMock
            .Setup(r => r.StreetcodeRepository.GetAllAsync(
                    It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                    It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync((
                Expression<Func<StreetcodeContent, bool>> expression,
                Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>> e) =>
            {
                var predicate = expression.Compile();
                streetcodes = streetcodes.Where(predicate).ToList();
    
                return streetcodes.Any() ? streetcodes : null!;
            });

        _mapperMock
            .Setup(m => m.Map<IEnumerable<RelatedFigureDto>>(It.IsAny<IEnumerable<RelatedFigure>>()))
            .Returns(relatedFigures);
    
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
    
        // Assert
        result.IsSuccess.Should().BeTrue();
        Assert.Equal(1, streetcodes[0].Id);
        _loggerMock.Verify(
            l => l.LogError(query, $"Cannot find any streetcode with corresponding tagid: {query.tagId}"),
            Times.Never);
    }
}