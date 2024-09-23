using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.Dto.Timeline;
using Streetcode.DAL.Entities.Streetcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Dto.Media.Art
{
    public class ArtCreateDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }
        public int ImageId { get; set; }
        public List<StreetcodeShortDto>? Streetcodes { get; set; } = new ();
    }
}
