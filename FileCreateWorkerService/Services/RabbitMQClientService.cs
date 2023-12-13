using RabbitMQ.Client;

namespace FileCreateWorkerService.Services;

public class RabbitMQClientService : IDisposable
{
    private readonly ConnectionFactory connectionFactory;
    private IConnection connection;
    private IModel channel;
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
