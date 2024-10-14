using MailKit.Net.Smtp;
using MimeKit;
using Streetcode.Email.Models;

namespace Streetcode.Email.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IConfiguration configuration)
        {
            _smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>(); ;
        }

        public async Task<bool> SendEmailAsync(SendMailDto sendMailDto)
        {
            try
            {
                using var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                emailMessage.To.Add(new MailboxAddress("", sendMailDto.To));
                emailMessage.Subject = sendMailDto.Subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = sendMailDto.Message
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.UseSsl);
                    await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch { return false; }
        }
    }
}
