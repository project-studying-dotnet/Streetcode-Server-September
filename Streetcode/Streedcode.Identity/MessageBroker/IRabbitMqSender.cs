namespace Streedcode.Identity.MessageBroker
{
    public interface IRabbitMqSender
    {
        void SendMessage(object message, string exchangeName, string queueName);
    }
}
