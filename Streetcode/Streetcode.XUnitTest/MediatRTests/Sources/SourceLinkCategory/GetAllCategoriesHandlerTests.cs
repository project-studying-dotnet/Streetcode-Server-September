using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using SrcLinkCategory = Streetcode.DAL.Entities.Sources.SourceLinkCategory;

namespace Streetcode.XUnitTest.MediatRTests.Sources.SourceLinkCategory;

public class GetAllCategoriesHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBlobAzureService> _blobServiceMock; 
    private readonly GetAllCategoriesHandler _handler;

    public GetAllCategoriesHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _blobServiceMock = new Mock<IBlobAzureService>(); 
        _handler = new GetAllCategoriesHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnOkResult_WhenSourceLinkCategoriesExist()
    {
        // Arrange
        var image = new Image { Id = 1, BlobName = "blob1" };
        var imageDto = new ImageDto { Id = 1, BlobName = "blob1" };
        var sourceLinkCategories = new List<SrcLinkCategory>
            {
                new SrcLinkCategory { Id = 1, Title = "Link Title", ImageId = 1, Image = image }
            };
        var mappedCategories = new List<SourceLinkCategoryDto>
            {
                new SourceLinkCategoryDto { Id = 1, Title = "Link Title", ImageId = 1, Image = imageDto }
            };

        _repositoryWrapperMock.Setup(repo => repo.SourceCategoryRepository
               .GetAllAsync(It.IsAny<Expression<Func<SrcLinkCategory, bool>>>(),
               It.IsAny<Func<IQueryable<SrcLinkCategory>, IIncludableQueryable<SrcLinkCategory, object>>>()))
            .ReturnsAsync(sourceLinkCategories);

        _blobServiceMock
            .Setup(blobService => blobService.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns("base64string"); 

        _mapperMock.Setup(m => m.Map<IEnumerable<SourceLinkCategoryDto>>(sourceLinkCategories))
            .Returns(mappedCategories);

        // Act
        var result = await _handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(mappedCategories.Count, result.Value.Count());

        foreach (var dto in mappedCategories)
        {
            _blobServiceMock.Verify(b => b.FindFileInStorageAsBase64(dto.Image.BlobName), Times.Once);
        }
    }

    [Fact]
    public async Task Handle_ReturnsFailResult_WhenCategoriesAreNull()
    {
        // Arrange
        _repositoryWrapperMock.Setup(repo => repo.SourceCategoryRepository
               .GetAllAsync(It.IsAny<Expression<Func<SrcLinkCategory, bool>>>(),
               It.IsAny<Func<IQueryable<SrcLinkCategory>, IIncludableQueryable<SrcLinkCategory, object>>>()))
            .ReturnsAsync((IEnumerable<SrcLinkCategory>)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => 
                        _handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None));

        Assert.Equal("Categories is null", exception.Message);
    }
}
