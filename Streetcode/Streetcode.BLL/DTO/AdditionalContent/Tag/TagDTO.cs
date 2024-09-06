using Streetcode.BLL.Dto.Streetcode;

namespace Streetcode.BLL.Dto.AdditionalContent;

public class TagDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public IEnumerable<StreetcodeDto> Streetcodes { get; set; }
}
