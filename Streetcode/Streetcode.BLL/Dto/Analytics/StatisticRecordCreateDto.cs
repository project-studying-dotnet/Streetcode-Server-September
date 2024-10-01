using Streetcode.BLL.Dto.AdditionalContent.Coordinates.Types;

namespace Streetcode.BLL.Dto.Analytics;

public class StatisticRecordCreateDto
{
    public int QrId { get; set; }
    public string Address { get; set; } = null!;
    public StreetcodeCoordinateDto StreetcodeCoordinate { get; set; } = null!;
}
