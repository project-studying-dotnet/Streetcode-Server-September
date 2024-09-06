namespace Streetcode.BLL.Dto.Toponyms;

public class GetAllToponymsResponseDto
{
    public int Pages { get; set; }
    public IEnumerable<ToponymDto> Toponyms { get; set; }
}