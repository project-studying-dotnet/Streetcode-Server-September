using Streetcode.BLL.Dto.AdditionalContent;

namespace Streetcode.BLL.Dto.Transactions;

public class TransactLinkDto
{
    public int Id { get; set; }
    public string? Url { get; set; }
    public string? QrCodeUrl { get; set; }
    public int StreetcodeId { get; set; }
}