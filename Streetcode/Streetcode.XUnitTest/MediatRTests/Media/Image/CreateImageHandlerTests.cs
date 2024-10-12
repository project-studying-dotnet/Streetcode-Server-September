using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Image.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;

namespace Streetcode.XUnitTest.MediatRTests.Media.Image;

public class CreateImageHandlerTests
{
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateImageHandler _handler;

    public CreateImageHandlerTests()
    {
        _blobAzureServiceMock = new Mock<IBlobAzureService>();
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _mapperMock = new Mock<IMapper>();

        _handler = new CreateImageHandler(
            _blobAzureServiceMock.Object,
            _repositoryWrapperMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenImageIsCreatedSuccessfully()
    {
        // Arrange
        var command = new CreateImageCommand(new ImageFileBaseCreateDto
        {
            BaseFormat = "base64String",
            Title = "testImage"
        });

        var imageEntity = new ImageEntity { BlobName = command.Image.Title };
        var createdImageDto = new ImageDto { BlobName = command.Image.Title, Base64 = "base64Image" };

        _blobAzureServiceMock
            .Setup(service => service.SaveFileInStorage(command.Image.BaseFormat, command.Image.Title, string.Empty));

        _mapperMock
            .Setup(mapper => mapper.Map<ImageEntity>(command.Image))
            .Returns(imageEntity);

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.CreateAsync(It.IsAny<ImageEntity>()))
            .ReturnsAsync(imageEntity);

        _repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(1);

        _mapperMock
            .Setup(mapper => mapper.Map<ImageDto>(imageEntity))
            .Returns(createdImageDto);

        _blobAzureServiceMock
            .Setup(service => service.FindFileInStorageAsBase64(command.Image.Title))
            .Returns("base64Image");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("base64Image", result.Value.Base64);
        _blobAzureServiceMock.Verify(service => service.SaveFileInStorage(command.Image.BaseFormat, command.Image.Title, string.Empty), Times.Once);
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64(command.Image.Title), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.CreateAsync(It.IsAny<ImageEntity>()), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenImageCreationFails()
    {
        // Arrange
        var command = new CreateImageCommand(new ImageFileBaseCreateDto
        {
            BaseFormat = "base64String",
            Title = "testImage"
        });

        var imageEntity = new ImageEntity { BlobName = command.Image.Title };
        var createdImageDto = new ImageDto { BlobName = command.Image.Title, Base64 = "base64Image" };

        _blobAzureServiceMock
            .Setup(service => service.SaveFileInStorage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();

        _mapperMock
            .Setup(mapper => mapper.Map<ImageEntity>(command.Image))
            .Returns(imageEntity);

        _mapperMock
            .Setup(mapper => mapper.Map<ImageDto>(imageEntity))
            .Returns(createdImageDto);

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.CreateAsync(It.IsAny<ImageEntity>()))
            .ReturnsAsync((ImageEntity)null);

        _repositoryWrapperMock
            .Setup(repo => repo.SaveChangesAsync())
            .ReturnsAsync(0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Failed to create an image", exception.Message);

        _blobAzureServiceMock.Verify(service => service.SaveFileInStorage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.ImageRepository.CreateAsync(It.IsAny<ImageEntity>()), Times.Once);
        _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }
}

