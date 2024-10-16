using Streetcode.Identity.Models.AzureBus;

namespace Streetcode.Identity.Services.Interfaces
{
    public interface IAzureBusService
    {
        Task PublishMessage(BaseMessage message, string topicName);
    }
}
