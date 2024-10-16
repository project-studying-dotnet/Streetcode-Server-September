using Streetcode.BLL.Dto.Streetcode.RelatedFigure;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedFigure.GetByStreetcodeId;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.RelatedFigure;

using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using Xunit;
using RelatedFigure = DAL.Entities.Streetcode.RelatedFigure;

public class GetRelatedFiguresByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly GetRelatedFiguresByStreetcodeIdHandler _handler;

    public GetRelatedFiguresByStreetcodeIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new GetRelatedFiguresByStreetcodeIdHandler(
            _mapperMock.Object,
            _repositoryWrapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailResult_WhenNoRelatedFigureIdsFound()
    {
        // Arrange
        const int Id = 0;
        string expectedErrorMsg = $"Cannot find any related figures by a streetcode id: {Id}";
        _repositoryWrapperMock
            .Setup(r => r.RelatedFigureRepository
                .FindAll(It.IsAny<Expression<Func<RelatedFigure, bool>>>()))
            .Returns((IQueryable<RelatedFigure>)null!);

        // Act
        var result = await _handler.Handle(new GetRelatedFigureByStreetcodeIdQuery(Id), default);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        _loggerMock.Verify(
            x => x.LogError(
                It.IsAny<GetRelatedFigureByStreetcodeIdQuery>(),
                It.Is<string>(s => s.Contains(expectedErrorMsg))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailResult_WhenNoRelatedFiguresFoundInRepository()
    {
        // Arrange
        const int Id = 0;
        string expectedErrorMsg = $"Cannot find any related figures by a streetcode id: {Id}";
        var request = new GetRelatedFigureByStreetcodeIdQuery(Id);
        var relatedFigure = new List<RelatedFigure>
        {
            new () { ObserverId = 1 },
            new () { TargetId = 2 },
        };

        _repositoryWrapperMock
            .Setup(repo => repo.RelatedFigureRepository
                .FindAll(It.IsAny<Expression<Func<RelatedFigure, bool>>>()))
            .Returns(relatedFigure.AsQueryable());

        _repositoryWrapperMock
            .Setup(repo => repo.StreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync((IEnumerable<StreetcodeContent>)null!);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Single(result.Errors);
        _repositoryWrapperMock.Verify(
            x => x.StreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.LogError(
                It.IsAny<GetRelatedFigureByStreetcodeIdQuery>(),
                It.Is<string>(s => s.Contains(expectedErrorMsg))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenRelatedFiguresAreFoundAndMapped()
    {
        // Arrange
        int streetcodeId = 1;
        var request = new GetRelatedFigureByStreetcodeIdQuery(streetcodeId);

        var relatedFigures = new List<RelatedFigure>
        {
            new ()
            {
                TargetId = 1,
                ObserverId = 2,
            },
        };

        var streetcodeFigures = new List<StreetcodeContent>
        {
            new ()
            {
                Id = 2,
                Images = new List<Image>
                {
                    new () { ImageDetails = new ImageDetails { Alt = "B", }, },
                    new () { ImageDetails = new ImageDetails { Alt = "A", }, },
                },
                Status = StreetcodeStatus.Published,
            },
        };

        _repositoryWrapperMock
            .Setup(repo => repo.RelatedFigureRepository
                .FindAll(It.IsAny<Expression<Func<RelatedFigure, bool>>>()))
            .Returns((Expression<Func<RelatedFigure, bool>> predicate) => relatedFigures.AsQueryable().Where(predicate));

        _repositoryWrapperMock
            .Setup(repo => repo.StreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync((
                Expression<Func<StreetcodeContent, bool>> exp,
                Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>> _) =>
            {
                var predicate = exp.Compile();
                var filteredFigures = streetcodeFigures.Where(predicate).ToList();

                return filteredFigures.Any() ? filteredFigures : null!;
            });

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<RelatedFigureDto>>(It.IsAny<IEnumerable<StreetcodeContent>>()))
            .Returns(new List<RelatedFigureDto>());

        // Act
        var result = await _handler.Handle(request, default);

        // Assert
        Assert.True(result.IsSuccess);
        _mapperMock.Verify(
            mapper => mapper.Map<IEnumerable<RelatedFigureDto>>(It.IsAny<IEnumerable<StreetcodeContent>>()),
            Times.Once);
        Assert.Equal("A", streetcodeFigures[0].Images.First().ImageDetails?.Alt);
    }
}