using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Media.Image.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;

namespace Streetcode.XUnitTest.MediatRTests.Media.Images;

public class GetImageByIdHandlerTests
{
    private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
    private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
    private readonly IMapper _mapper;
    private readonly GetImageByIdHandler _handler;

    public GetImageByIdHandlerTests()
    {
        _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
        _blobAzureServiceMock = new Mock<IBlobAzureService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ImageEntity, ImageDto>();
        });
        _mapper = config.CreateMapper();

        _handler = new GetImageByIdHandler(_repositoryWrapperMock.Object, _mapper, _blobAzureServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnImageDto_WhenImageExists()
    {
        // Arrange
        var query = new GetImageByIdQuery(1);
        var image = new ImageEntity
        {
            Id = 1,
            BlobName = "testBlob"
        };

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()))
            .ReturnsAsync(image);

        _blobAzureServiceMock
            .Setup(service => service.FindFileInStorageAsBase64("testBlob"))
            .Returns("base64String");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var imageDto = result.Value;
        Assert.NotNull(imageDto);
        Assert.Equal("base64String", imageDto.Base64);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomException_WhenImageDoesNotExist()
    {
        // Arrange
        var query = new GetImageByIdQuery(1);

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                null))
            .ReturnsAsync((ImageEntity)null);

        // Act
        var exception = await Assert.ThrowsAsync<CustomException>(
            async () => await _handler.Handle(query, CancellationToken.None));

        // Assert
        Assert.Equal("Cannot find a image with corresponding id: 1", exception.Message);
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldCallBlobService_WhenBlobNameIsNotNull()
    {
        // Arrange
        var query = new GetImageByIdQuery (1);
        var image = new ImageEntity
        {
            Id = 1,
            BlobName = "testBlob"
        };

        _repositoryWrapperMock
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<ImageEntity, bool>>>(),
                It.IsAny<Func<IQueryable<ImageEntity>, IIncludableQueryable<ImageEntity, object>>>()))
            .ReturnsAsync(image);

        _blobAzureServiceMock
            .Setup(service => service.FindFileInStorageAsBase64("testBlob"))
            .Returns("base64String");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _blobAzureServiceMock.Verify(service => service.FindFileInStorageAsBase64("testBlob"), Times.Once);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("base64String", result.Value.Base64);
    }
}