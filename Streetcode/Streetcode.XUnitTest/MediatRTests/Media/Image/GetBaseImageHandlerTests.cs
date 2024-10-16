using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Image.GetBaseImage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;

namespace Streetcode.XUnitTest.MediatRTests.Media.Image;

public class GetBaseImageHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly GetBaseImageHandler _handler;

    public GetBaseImageHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _handler = new GetBaseImageHandler(_repositoryWrapperMock.Object, _blobAzureServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMemoryStream_WhenImageIsFound()
    {
        // Arrange
        var imageId = 1;
        var image = new ImageEntity { Id = imageId, BlobName = "imageBlobName" };
        var memoryStream = new MemoryStream();

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository
            .GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()))
            .ReturnsAsync(image);

        _blobAzureServiceMock
            .Setup(service => service.FindFileInStorageAsMemoryStream(image.BlobName))
            .Returns(memoryStream);

        // Act
        var result = await _handler.Handle(new GetBaseImageQuery(imageId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(memoryStream, result.Value);
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository
       .GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsMemoryStream(image.BlobName), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenImageIsNotFound()
    {
        // Arrange
        var imageId = 1;

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()))
            .ReturnsAsync((ImageEntity)null); 

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(new GetBaseImageQuery(imageId), CancellationToken.None));

        Assert.Equal($"Cannot find an image with corresponding id: {imageId}", exception.Message);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode); 
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()), Times.Once);
    }
}
