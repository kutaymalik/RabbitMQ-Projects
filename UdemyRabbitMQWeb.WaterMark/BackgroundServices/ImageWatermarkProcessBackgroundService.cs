using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Drawing;
using System.Text;
using System.Text.Json;
using UdemyRabbitMQWeb.WaterMark.Services;

namespace UdemyRabbitMQWeb.WaterMark.BackgroundServices;

public class ImageWatermarkProcessBackgroundService : BackgroundService
{
    private readonly RabbitMQClientService rabbitMQClientService;
    private readonly ILogger<ImageWatermarkProcessBackgroundService> logger;
    private IModel channel;

    public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
    {
        this.rabbitMQClientService = rabbitMQClientService;
        this.logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        channel = rabbitMQClientService.Connect();
        channel.BasicQos(0, 1, false);


        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Async Reading
        var consumer = new AsyncEventingBasicConsumer(channel);

        //channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

        consumer.Received += Consumer_Received;

        channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

        return Task.CompletedTask;
    }

    private async Task<Task> Consumer_Received(object sender, BasicDeliverEventArgs @event)
    {

        await Task.Delay(5000);

        try
        {
            var productImageCreatedEvent = JsonSerializer.Deserialize<productImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", productImageCreatedEvent.ImageName);

            var siteName = "biMilyoncuMemet";

            using var img = Image.FromFile(path);
            using var graphic = Graphics.FromImage(img);
            var font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);

            var textSize = graphic.MeasureString(siteName, font);
            var color = Color.FromArgb(128, 255, 255, 255);
            var brush = new SolidBrush(color);

            var position = new Point(img.Width - ((int)textSize.Width + 40), img.Height - ((int)textSize.Height + 30));

            graphic.DrawString(siteName, font, brush, position);

            img.Save("wwwroot/Images/watermarks/" + productImageCreatedEvent.ImageName);

            img.Dispose();
            graphic.Dispose();

            channel.BasicAck(@event.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}
