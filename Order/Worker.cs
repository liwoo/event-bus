using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Order
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly string _topic;

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _topic = _config.GetValue<string>("RabbitMQ:topic");
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _config.GetValue<string>("RabbitMQ:Connection"),
                UserName = _config.GetValue<string>("RabbitMQ:Username"),
                Password = _config.GetValue<string>("RabbitMQ:Password")
            };


            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: _topic, type: "topic");
            var queueName = channel.QueueDeclare().QueueName;

            var bindingKey = "basket.*";

            channel.QueueBind(queue: queueName,
                                  exchange: _topic,
                                  routingKey: bindingKey);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var action = ea.RoutingKey.Split('.')[1];

                var (word, icon) = GetDoingWordIcon(action);

                _logger.LogInformation(" [{0}] {1} order for {2}", icon, word, message);

            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            await Task.Delay(1000, stoppingToken);

        }
        private (string, string) GetDoingWordIcon(string action)
        {
            return action switch
            {
                "created" => ("Creating", "+"),
                "deleted" => ("Removing", "-"),
                "updated" => ("Updating", ">"),
                _ => ("Disacrding", "_")
            };
        }

        //TODO: Lift the connection and channel to class fields and dispose them here
    }

}
