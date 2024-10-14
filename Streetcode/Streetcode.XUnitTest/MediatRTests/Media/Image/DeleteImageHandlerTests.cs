using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Image.Delete;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;
namespace Streetcode.XUnitTest.MediatRTests.Media.Image;

public class DeleteImageHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly DeleteImageHandler _handler;

    public DeleteImageHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _handler = new DeleteImageHandler(_repositoryWrapperMock.Object, _blobAzureServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenImageIsDeletedSuccessfully()
    {
        // Arrange
        var imageId = 1;
        var image = new ImageEntity { Id = imageId, BlobName = "testBlob" };

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()))
            .ReturnsAsync(image);

        _repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1); 

        // Act
        var result = await _handler.Handle(new DeleteImageCommand(imageId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.Delete(It.IsAny<ImageEntity>()), Times.Once);
        _blobAzureServiceMock.Verify(service => service.DeleteFileInStorage(It.IsAny<string>()), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenImageNotFound()
    {
        // Arrange
        var imageId = 1;

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()))
            .ReturnsAsync((ImageEntity?)null); 

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(new DeleteImageCommand(imageId), CancellationToken.None));

        Assert.Equal($"Cannot find an image with corresponding categoryId: {imageId}", exception.Message);
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.Delete(It.IsAny<ImageEntity>()), Times.Never);
        _blobAzureServiceMock.Verify(service => service.DeleteFileInStorage(It.IsAny<string>()), Times.Never);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenImageDeletionFails()
    {
        // Arrange
        var imageId = 1;
        var image = new ImageEntity { Id = imageId, BlobName = "testBlob" };

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()))
            .ReturnsAsync(image);

        _repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0); 

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(new DeleteImageCommand(imageId), CancellationToken.None));

        Assert.Equal("Failed to delete an image", exception.Message);
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.Delete(It.IsAny<ImageEntity>()), Times.Once);
        _blobAzureServiceMock.Verify(service => service.DeleteFileInStorage(It.IsAny<string>()), Times.Never);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}