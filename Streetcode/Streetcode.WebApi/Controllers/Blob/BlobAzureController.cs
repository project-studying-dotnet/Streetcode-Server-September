using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Services.BlobStorageService;

namespace Streetcode.WebApi.Controllers.Blob;

[ApiController]
[Route("api/[controller]")]
public class BlobAzureController : ControllerBase
{
    private readonly BlobAzureService _blobServiceAzure;
    private readonly BlobServiceClient _blobServiceClient;

    public BlobAzureController(IOptions<BlobAzureVariables> environment)
    {
        _blobServiceClient = new BlobServiceClient(environment.Value.ConnectionString);
        _blobServiceAzure = new BlobAzureService(environment, _blobServiceClient);
    }

    [HttpPost("upload")]
    public IActionResult UploadFile([FromForm] string base64File, [FromForm] string fileName)
    {
        var fileUrl = _blobServiceAzure.SaveFileInStorage(base64File, fileName, string.Empty);
        return Ok(new { fileUrl });
    }

    [HttpGet("downloadstream/{name}")]
    public IActionResult DownloadFileAsStream(string name)
    {
        var mimeType = _blobServiceAzure.GetMimeType(name);
        var memoryStream = _blobServiceAzure.FindFileInStorageAsMemoryStream(name);
        return File(memoryStream, mimeType, name);
    }

    [HttpGet("download/{name}")]
    public IActionResult DownloadFile(string name)
    {
        var base64 = _blobServiceAzure.FindFileInStorageAsBase64(name);
        return Ok(base64);
    }

    [HttpDelete("{name}")]
    public IActionResult DeleteFile(string name)
    {
        _blobServiceAzure.DeleteFileInStorage(name);
        return Ok();
    }
}
