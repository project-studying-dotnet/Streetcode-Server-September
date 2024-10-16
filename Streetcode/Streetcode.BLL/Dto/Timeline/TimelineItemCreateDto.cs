using Streetcode.DAL.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Dto.Timeline
{
    public class TimelineItemCreateDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public DateViewPattern DateViewPattern { get; set; }
        public IEnumerable<HistoricalContextDto>? HistoricalContexts { get; set; } = new List<HistoricalContextDto>();
        public int StreetcodeId { get; set; }
    }
}
