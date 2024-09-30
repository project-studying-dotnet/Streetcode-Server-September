using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Streedcode.Identity.MessageBroker
{
    public class RabbitMqSender : IRabbitMqSender
    {
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;

        public RabbitMqSender(IConfiguration configuration)
        {
            var rabbitMqSection = configuration.GetSection("RabbitMq");

            _hostName = rabbitMqSection.GetValue<string>("HostName");
            _username = rabbitMqSection.GetValue<string>("Username");
            _password = rabbitMqSection.GetValue<string>("Password");

            CreateConnection();
        }

        public void SendMessage(object message, string exchangeName, string queueName)
        {
            if (!ConnectionExists())
            {
                throw new InvalidOperationException("RabbitMQ connection could not be established.");
            }

            using var channel = _connection.CreateModel();

            channel.ExchangeDeclare(exchange: exchangeName, type: "direct");
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: queueName);

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish(exchange: exchangeName, routingKey: queueName, basicProperties: null, body: body);
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostName,
                    Password = _password,
                    UserName = _username
                };

                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {

            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }
            CreateConnection();
            return true;
        }
    }
}
