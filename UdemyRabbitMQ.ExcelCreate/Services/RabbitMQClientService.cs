using RabbitMQ.Client;

namespace UdemyRabbitMQ.ExcelCreate.Services;

public class RabbitMQClientService : IDisposable
{
    private readonly ConnectionFactory connectionFactory;
    private IConnection connection;
    private IModel channel;
    public static string ExchangeName = "ExcelDirectExchange";
    public static string RoutingExcel = "excel-route-file";
    public static string QueueName = "queue-excel-file";
    private readonly ILogger<RabbitMQClientService> logger;

    public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
    {
        this.connectionFactory = connectionFactory;
        this.logger = logger;
    }

    public IModel Connect()
    {
        connection = connectionFactory.CreateConnection();

        if (channel is { IsOpen: true })
        {
            return channel;
        }

        channel = connection.CreateModel();

        channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);

        channel.QueueDeclare(QueueName, true, false, false, null);

        channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingExcel);

        logger.LogInformation("System connected to RabbitMQ");

        return channel;
    }

    public void Dispose()
    {
        channel?.Close();
        channel?.Dispose();

        connection?.Close();
        connection?.Dispose();

        logger.LogInformation("System disconnected to RabbitMQ");
    }
}
