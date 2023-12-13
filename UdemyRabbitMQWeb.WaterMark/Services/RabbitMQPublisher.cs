using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace UdemyRabbitMQWeb.WaterMark.Services;

public class RabbitMQPublisher
{
    private readonly RabbitMQClientService rabbitMQClientService;

    public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
    {
        this.rabbitMQClientService = rabbitMQClientService;
    }

    public void Publish(productImageCreatedEvent productImageCreatedEvent)
    {
        var channel = rabbitMQClientService.Connect();
        
        var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);
        var bodyByte = Encoding.UTF8.GetBytes(bodyString);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(RabbitMQClientService.ExchangeName, RabbitMQClientService.RoutingWatermark, basicProperties: properties, body: bodyByte);
    }
}
