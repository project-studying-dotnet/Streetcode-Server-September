namespace Streetcode.BLL.Dto.Toponyms;

public class GetAllToponymsRequestDto
{
    public int Page { get; set; } = 1;
    public int Amount { get; set; } = 10;
    public string? Title { get; set; } = null;
}