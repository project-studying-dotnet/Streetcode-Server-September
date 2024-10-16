using Streetcode.Identity.Enums;

namespace Streetcode.Identity.Models.AzureBus
{
    public class AzureRegisterMailDto : BaseMessage
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public EmailType EmailType { get; set; }

    }
}
