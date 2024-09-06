namespace Streetcode.BLL.Dto.Streetcode;

public class GetAllStreetcodesResponseDto
{
    public int Pages { get; set; }
    public IEnumerable<StreetcodeDto> Streetcodes { get; set; }
}
