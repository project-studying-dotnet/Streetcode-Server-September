using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Moq;
using Streetcode.BLL.Services.BlobStorageService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Azure;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Azure.Storage;

namespace Streetcode.XUnitTest.Services;

public class BlobAzureServiceTests
{
    private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
    private readonly Mock<BlobContainerClient> _mockContainerClient;
    private readonly Mock<BlobClient> _mockBlobClient;
    private readonly Mock<IOptions<BlobAzureVariables>> _mockOptions;
    private readonly BlobAzureService _blobAzureService;

    public BlobAzureServiceTests()
    {
        _mockBlobServiceClient = new Mock<BlobServiceClient>();
        _mockContainerClient = new Mock<BlobContainerClient>();
        _mockBlobClient = new Mock<BlobClient>();
        _mockOptions = new Mock<IOptions<BlobAzureVariables>>();

        _mockOptions.Setup(x => x.Value).Returns(new BlobAzureVariables
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "test-container"
        });

        _mockBlobServiceClient
            .Setup(client => client.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_mockContainerClient.Object);

        _mockContainerClient
          .Setup(client => client.GetBlobClient(It.IsAny<string>()))
          .Returns(_mockBlobClient.Object);

        _mockBlobClient
          .Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<BlobHttpHeaders>(), It.IsAny<IDictionary<string, string>>(),
              It.IsAny<BlobRequestConditions>(), It.IsAny<IProgress<long>>(), It.IsAny<AccessTier?>(),
              It.IsAny<StorageTransferOptions>(), It.IsAny<CancellationToken>()))
          .Returns(new Mock<Response<BlobContentInfo>>().Object);

        _blobAzureService = new BlobAzureService(_mockOptions.Object, _mockBlobServiceClient.Object);
    }

    [Fact]
    public void DeleteFileInStorage_ValidFileName_ShouldCallDeleteIfExists()
    {
        // Arrange
        string fileName = "test.png";

        _mockBlobClient
            .Setup(x => x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(),
                                          It.IsAny<BlobRequestConditions>(),
                                          It.IsAny<CancellationToken>()))
            .Returns(Response.FromValue(true, new Mock<Response>().Object)); 

        // Act
        _blobAzureService.DeleteFileInStorage(fileName);

        // Assert
        _mockBlobClient.Verify(x => x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(),
                                                      It.IsAny<BlobRequestConditions>(),
                                                      It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void FindFileInStorageAsBase64_ValidFileName_ShouldReturnBase64String()
    {
        //Arrange
        string fileName = "test.png";
        byte[] fileContent = Encoding.UTF8.GetBytes("File content");
        var stream = new MemoryStream(fileContent);

        var binaryData = BinaryData.FromBytes(fileContent);
        var blobDownloadResult = BlobsModelFactory.BlobDownloadResult(content: binaryData);
        var mockResponse = Response.FromValue(blobDownloadResult, Mock.Of<Response>());

        _mockBlobClient.Setup(x => x.DownloadContent()).Returns(mockResponse);

        //Act
        var result = _blobAzureService.FindFileInStorageAsBase64(fileName);

        //Assert
        Assert.Equal(Convert.ToBase64String(fileContent), result);
    }

    [Fact]
    public void FindFileInStorageAsMemoryStream_ValidFileName_ShouldReturnMemoryStream()
    {
        //Arrange
        string fileName = "test.png";
        byte[] fileContent = Encoding.UTF8.GetBytes("File content");

        var binaryData = BinaryData.FromBytes(fileContent);

        var blobDownloadResult = BlobsModelFactory.BlobDownloadResult(content: binaryData);

        var mockResponse = Response.FromValue(blobDownloadResult, Mock.Of<Response>());

        _mockBlobClient.Setup(x => x.DownloadContent())
            .Returns(mockResponse);

        // Act
        var result = _blobAzureService.FindFileInStorageAsMemoryStream(fileName);

        // Assert
        Assert.IsType<MemoryStream>(result);
        using (var reader = new StreamReader(result))
        {
            string content = reader.ReadToEnd();
            Assert.Equal("File content", content);
        }
    }

    [Fact]
    public void GetMimeType_InvalidFileExtension_ShouldThrowCustomException()
    {
        // Arrange
        string invalidFileName = "test.txt";

        // Act & Assert
        var exception = Assert.Throws<CustomException>(() => _blobAzureService.GetMimeType(invalidFileName));
        Assert.Equal("Invalid file name. The name must include a valid extension " +
                     "(.mp3, .jpeg, .jpg, .png, .gif).", exception.Message);
    }

    [Fact]
    public void ValidateFileName_InvalidFileName_ShouldThrowCustomException()
    {
        // Arrange
        string invalidFileName = "test.txt";

        // Act & Assert
        var exception = Assert.Throws<CustomException>(() => _blobAzureService.ValidateFileName(invalidFileName));
        Assert.Equal("Invalid file name. The name must include a valid extension " +
                     "(.mp3, .jpeg, .jpg, .png, .gif).", exception.Message);
    }
}