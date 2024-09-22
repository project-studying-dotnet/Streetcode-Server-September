using Streetcode.BLL.Dto.Media.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Dto.Media.Art
{
    public class ArtCreateDto
    {
        public string? Description { get; set; }
        public string? Title { get; set; }
        public int ImageId { get; set; }
        public int StreetcodeId { get; set; }
    }
}
