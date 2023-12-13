using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace UdemyRabbitMQ.subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();

            factory.Uri = new Uri("amqps://rlgpymfe:mYyzyJy_R0k6U8l5cOhG_7ZmKPlvjfXI@hawk.rmq.cloudamqp.com/rlgpymfe");

            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);

            // durable=false = in-memory -- exclusive=true (only this channel can use this queue) -- autoDelete=true (If the subscriber is down, the queue will be deleted.)
            var queueName = channel.QueueDeclare().QueueName;

            Dictionary<string, object> headers = new Dictionary<string, object>();

            headers.Add("format", "pdf");
            headers.Add("shape", "a4");
            //headers.Add("x-match", "all");
            headers.Add("x-match", "any");

            channel.QueueBind(queueName, "header-exchange", string.Empty, headers);

            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Logs listening...");

            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());

                Product product = JsonSerializer.Deserialize<Product>(message);

                Thread.Sleep(1000);
                Console.WriteLine($"Received Message: {product.Id} - {product.Name} - {product.Price}  - {product.Stock}");

                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }
    }
}