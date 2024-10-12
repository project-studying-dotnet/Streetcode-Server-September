using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;

namespace Streetcode.BLL.Services.BlobStorageService;

public class BlobAzureService : IBlobAzureService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly BlobAzureVariables _environment;
    private readonly string _containerName;

    public BlobAzureService(IOptions<BlobAzureVariables> environment, BlobServiceClient blobServiceClient)
    { 
      _environment = environment.Value;
      _containerName = _environment.ContainerName;

      _blobServiceClient = blobServiceClient;
      _containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
      _containerClient.CreateIfNotExists();
    }

    public void DeleteFileInStorage(string name)
    {
        ValidateFileName(name);
        var blobClient = _containerClient.GetBlobClient(name);
        blobClient.DeleteIfExists();
    }

    public string FindFileInStorageAsBase64(string name)
    {
        ValidateFileName(name);
        var blobClient = _containerClient.GetBlobClient(name);
        var downloadInfo = blobClient.DownloadContent();
        byte[] fileBytes = downloadInfo.Value.Content.ToArray();
        return Convert.ToBase64String(fileBytes);
    }

    public MemoryStream FindFileInStorageAsMemoryStream(string name)
    {
        ValidateFileName(name);
        var blobClient = _containerClient.GetBlobClient(name);
        var downloadInfo = blobClient.DownloadContent() ??
            throw new CustomException("Such blob wasn't found", StatusCodes.Status404NotFound);
        var memoryStream = new MemoryStream(downloadInfo.Value.Content.ToArray());
        return memoryStream;
    }

    public string SaveFileInStorage(string base64, string name, string mimeType = null!)
    {
        mimeType = GetMimeType(name);

        byte[] fileData = Convert.FromBase64String(base64);
        var blobClient = _containerClient.GetBlobClient(name);

        using var stream = new MemoryStream(fileData);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = mimeType 
        };

        blobClient.Upload(content: stream, new BlobHttpHeaders { ContentType = mimeType });

        return blobClient.Uri.ToString();
    }

    public string UpdateFileInStorage(string previousBlobName, string base64Format, string newBlobName, string extension)
    {
        var newMimeType = GetMimeType(newBlobName);
        DeleteFileInStorage(previousBlobName);
        return SaveFileInStorage(base64Format, newBlobName + extension, newMimeType);
    }

    public string GetMimeType(string name)
    {
        string fileExtension = Path.GetExtension(name);

        if (string.IsNullOrEmpty(fileExtension) || !_mimeTypeMap.ContainsKey(fileExtension))
        {
            throw new CustomException("Invalid file name. The name must include a valid extension " +
                "(.mp3, .jpeg, .jpg, .png, .gif).", StatusCodes.Status400BadRequest);
        }

        return _mimeTypeMap[fileExtension];    
    }

    public void ValidateFileName(string name)
    {
        if (string.IsNullOrEmpty(Path.GetExtension(name)) || !_mimeTypeMap.ContainsKey(Path.GetExtension(name)))
        {
            throw new CustomException("Invalid file name. The name must include a valid extension " +
                "(.mp3, .jpeg, .jpg, .png, .gif).", StatusCodes.Status400BadRequest);
        }
    }

    private readonly Dictionary<string, string> _mimeTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { ".mp3", "audio/mpeg" },
        { ".jpeg", "image/jpeg" },
        { ".jpg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
    };
}
