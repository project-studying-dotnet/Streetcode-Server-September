using Streetcode.Email.Enums;

namespace Streetcode.Email.Models
{
    public class SendMailDto : BaseMessage
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public EmailType EmailType { get; set; }
    }
}
