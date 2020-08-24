using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;

namespace ShoppingBasket
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

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: _topic, type: "topic");
                string[] actions = { "created", "deleted", "updated" };
                var rand = new Random();
                var count = 0;

                while (!stoppingToken.IsCancellationRequested)
                {
                    var routingKey = $"basket.{actions[rand.Next(actions.Length)]}";
                    string message = $"Basket # {count}";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: _topic,
                                        routingKey: routingKey,
                                        basicProperties: null,
                                        body: body
                                        );

                    _logger.LogInformation(" [x] Sent {0} on {1}", message, routingKey);
                    count += 1;
                    await Task.Delay(1000, stoppingToken);
                }

            }
        }
    }
}
