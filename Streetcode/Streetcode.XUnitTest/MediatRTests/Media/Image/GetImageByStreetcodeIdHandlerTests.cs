using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Image.GetByStreetcodeId;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;

namespace Streetcode.XUnitTest.MediatRTests.Media.Image;

public class GetImageByStreetcodeIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly GetImageByStreetcodeIdHandler _handler;

    public GetImageByStreetcodeIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _handler = new GetImageByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobAzureServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnImageDtos_WhenImagesAreFound()
    {
        // Arrange
        var streetcodeId = 1;
        var images = new List<ImageEntity>
        {
            new ImageEntity
            {
                Id = 1,
                BlobName = "blobName1",
                ImageDetails = new ImageDetails { Id = 1, Title = "Image 1", ImageId = 1} 
            },
            new ImageEntity
            {
                Id = 2,
                BlobName = "blobName2",
                ImageDetails = new ImageDetails { Id = 2, Title = "Image 2", ImageId = 2 }
            }
        };

        var imageDtos = new List<ImageDto>
        {
            new ImageDto { BlobName = "blobName1" },
            new ImageDto { BlobName = "blobName2" }
        };

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository
            .GetAllAsync(It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                          It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()))
            .ReturnsAsync(images);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<ImageDto>>(It.IsAny<IEnumerable<ImageEntity>>()))
            .Returns(imageDtos);

        _blobAzureServiceMock
            .Setup(service => service.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns("base64String");

        // Act
        var result = await _handler.Handle(new GetImageByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(imageDtos.Count, result.Value.Count());
        Assert.Equal("base64String", imageDtos.First().Base64);
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.GetAllAsync(It.IsAny<Expression<Func<ImageEntity, bool>>>(),
            It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<ImageDto>>(images), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Exactly(imageDtos.Count));
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenNoImagesAreFound()
    {
        // Arrange
        var streetcodeId = 1;

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetAllAsync(It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                                        It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()))
            .ReturnsAsync(new List<ImageEntity>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(new GetImageByStreetcodeIdQuery(streetcodeId), CancellationToken.None));

        Assert.Equal($"Cannot find an image with the corresponding streetcode id: {streetcodeId}", exception.Message);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode); 
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.GetAllAsync(It.IsAny<Expression<Func<ImageEntity, bool>>>(),
            It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()), Times.Once);
    }
}
