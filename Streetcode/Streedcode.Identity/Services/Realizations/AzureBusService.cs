using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Streetcode.Identity.Models.AzureBus;
using Streetcode.Identity.Services.Interfaces;
using System.Text;

namespace Streetcode.Identity.Services.Realizations
{
    public class AzureBusService : IAzureBusService
    {
        private readonly IConfiguration _configuration;

        private readonly string serviceBusConnectionString;
        private readonly string EmailMessageTopic;

        public AzureBusService(IConfiguration configuration)
        {
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusSettings:ServiceBusConnectionString");
            EmailMessageTopic = _configuration.GetValue<string>("ServiceBusSettings:EmailMessageTopic");
        }

        public async Task PublishMessage(BaseMessage message, string topicName)
        {

            await using var client = new ServiceBusClient(serviceBusConnectionString);

            ServiceBusSender sender = client.CreateSender(topicName);

            var jsonMessage = JsonConvert.SerializeObject(message);
            ServiceBusMessage finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            await sender.SendMessageAsync(finalMessage);

            await client.DisposeAsync();
        }
    }
}
