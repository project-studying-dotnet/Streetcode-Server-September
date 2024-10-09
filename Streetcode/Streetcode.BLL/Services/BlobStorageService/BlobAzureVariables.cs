using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Services.BlobStorageService;

public class BlobAzureVariables
{
    public string ConnectionString { get; set; } = null!;
    public string ContainerName { get; set; } = null!;
}
