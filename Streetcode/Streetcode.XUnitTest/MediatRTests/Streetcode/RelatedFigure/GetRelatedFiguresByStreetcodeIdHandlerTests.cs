using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.RelatedFigure;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedFigure.GetByStreetcodeId;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.RelatedFigure;

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
                .FindAll(It.IsAny<Expression<Func<DAL.Entities.Streetcode.RelatedFigure, bool>>>()))
            .Returns((IQueryable<DAL.Entities.Streetcode.RelatedFigure>)null);

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
        var relatedFigure = new List<DAL.Entities.Streetcode.RelatedFigure>
        {
            new () { ObserverId = 1 },
            new () { TargetId = 2 },
        };

        _repositoryWrapperMock
            .Setup(repo => repo.RelatedFigureRepository
                .FindAll(It.IsAny<Expression<Func<DAL.Entities.Streetcode.RelatedFigure, bool>>>()))
            .Returns(relatedFigure.AsQueryable());

        _repositoryWrapperMock
            .Setup(repo => repo.StreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync((IEnumerable<StreetcodeContent>)null);

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
        var streetcodeFigures = new List<StreetcodeContent>
        {
            new StreetcodeContent
            {
                Id = 1,
                Images = new List<Image>
                {
                    new () { ImageDetails = new ImageDetails() { Alt = "B", }, },
                    new () { ImageDetails = new ImageDetails() { Alt = "A", }, },
                },
            },
        };

        _repositoryWrapperMock
            .Setup(repo => repo.RelatedFigureRepository
                .FindAll(It.IsAny<Expression<Func<DAL.Entities.Streetcode.RelatedFigure, bool>>>()))
            .Returns(Enumerable.Empty<DAL.Entities.Streetcode.RelatedFigure>().AsQueryable());

        _repositoryWrapperMock
            .Setup(repo => repo.StreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(streetcodeFigures);

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<RelatedFigureDTO>>(It.IsAny<IEnumerable<StreetcodeContent>>()))
            .Returns(new List<RelatedFigureDTO>());

        // Act
        var result = await _handler.Handle(request, default);

        //Assert
        Assert.True(result.IsSuccess);
        _mapperMock.Verify(
            mapper => mapper.Map<IEnumerable<RelatedFigureDTO>>(It.IsAny<IEnumerable<StreetcodeContent>>()),
            Times.Once);
        Assert.Equal("A", streetcodeFigures[0].Images.First().ImageDetails?.Alt);
    }
}