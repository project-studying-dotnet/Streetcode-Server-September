using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Image.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;
namespace Streetcode.XUnitTest.MediatRTests.Media.Image;

public class GetAllImagesHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetAllImagesHandler _handler;

    public GetAllImagesHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAllImagesHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobAzureServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenImagesAreFound()
    {
        // Arrange
        var images = new List<ImageEntity>
        {
            new ImageEntity { Id = 1, BlobName = "blob1" },
            new ImageEntity { Id = 2, BlobName = "blob2" }
        };

        var imageDtos = new List<ImageDto>
        {
            new ImageDto { Id = 1, BlobName = "blob1" },
            new ImageDto { Id = 2, BlobName = "blob2" }
        };

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetAllAsync(It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                null))
            .ReturnsAsync(images);

        _mapperMock
            .Setup(mapper => mapper.Map<IEnumerable<ImageDto>>(images))
            .Returns(imageDtos);

        _blobAzureServiceMock
            .Setup(service => service.FindFileInStorageAsBase64(It.IsAny<string>()))
            .Returns("base64string");

        // Act
        var result = await _handler.Handle(new GetAllImagesQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.GetAllAsync(It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                null), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<ImageDto>>(It.IsAny<IEnumerable<ImageEntity>>()), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenNoImagesFound()
    {
        // Arrange
        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetAllAsync(It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                null))
            .ReturnsAsync((IEnumerable<ImageEntity>?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(new GetAllImagesQuery(),
                                                                                                            CancellationToken.None));

        Assert.Equal("Cannot find any image", exception.Message);
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.GetAllAsync(It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                null), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<ImageDto>>(It.IsAny<IEnumerable<ImageEntity>>()), Times.Never);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64(It.IsAny<string>()), Times.Never);
    }
}