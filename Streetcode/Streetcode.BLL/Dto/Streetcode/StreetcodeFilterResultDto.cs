namespace Streetcode.BLL.Dto.Streetcode
{
    public class StreetcodeFilterResultDto
    {
        public int StreetcodeId { get; set; }
        public string StreetcodeTransliterationUrl { get; set; } = null!;
        public int StreetcodeIndex { get; set; }
        public string? BlockName { get; set; }
        public string Content { get; set; } = null!;
        public string? SourceName { get; set; }
    }
}
