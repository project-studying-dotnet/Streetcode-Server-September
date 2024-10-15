using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Streetcode.Email.Enums;
using Streetcode.Email.Models;
using Streetcode.Email.Resources;
using Streetcode.Email.Services;
using System.Text;

namespace Streetcode.Email.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionEmailMessage;
        private readonly string EmailMessageTopic;

        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;        
        
        private readonly ServiceBusProcessor emailMessageProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusSettings:ServiceBusConnectionString");
            subscriptionEmailMessage = _configuration.GetValue<string>("ServiceBusSettings:SubscriptionEmailMessage");
            EmailMessageTopic = _configuration.GetValue<string>("ServiceBusSettings:EmailMessageTopic");


            var client = new ServiceBusClient(serviceBusConnectionString);
            emailMessageProcessor = client.CreateProcessor(EmailMessageTopic, subscriptionEmailMessage);
        }

        private async Task OnEmailMessageReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            SendMailDto sendMailDto = JsonConvert.DeserializeObject<SendMailDto>(body) ?? new SendMailDto();

            try
            {
                if(sendMailDto.EmailType == EmailType.Register)
                {
                    sendMailDto.Message = EmailTemplates.GetRegistrationEmailTemplate(sendMailDto.To, "https://streetcode.com.ua/");
                    await _emailService.SendEmailAsync(sendMailDto);
                }
                else
                {
                    await _emailService.SendEmailAsync(sendMailDto);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task Start()
        {
            emailMessageProcessor.ProcessMessageAsync += OnEmailMessageReceived;
            emailMessageProcessor.ProcessErrorAsync += ErrorHandler;
            await emailMessageProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        {
            await emailMessageProcessor.StopProcessingAsync();
            await emailMessageProcessor.DisposeAsync();
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
