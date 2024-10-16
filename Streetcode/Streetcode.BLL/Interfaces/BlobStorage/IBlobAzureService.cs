using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Interfaces.BlobStorage;

public interface IBlobAzureService
{
    public string SaveFileInStorage(string base64, string name, string mimeType);
    public MemoryStream FindFileInStorageAsMemoryStream(string name);
    public string UpdateFileInStorage(
        string previousBlobName,
        string base64Format,
        string newBlobName,
        string extension);
    public string FindFileInStorageAsBase64(string name);
    public void DeleteFileInStorage(string name);
}
