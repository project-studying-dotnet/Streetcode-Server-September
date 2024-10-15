using Streetcode.Email.Models;

namespace Streetcode.Email.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(SendMailDto sendMailDto);
    }
}
