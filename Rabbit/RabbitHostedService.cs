using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace TNRD.Zeepkist.GTR.Backend.Rabbit;

internal class RabbitHostedService : IHostedService
{
    private readonly RabbitOptions options;
    private readonly IRabbitPublisher publisher;

    private IConnection connection = null!;
    private IModel channel = null!;

    public RabbitHostedService(IOptions<RabbitOptions> options, IRabbitPublisher publisher)
    {
        this.publisher = publisher;
        this.options = options.Value;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ConnectionFactory factory = new ConnectionFactory()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.Username,
            Password = options.Password
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        channel.ExchangeDeclare("records", type: ExchangeType.Fanout);
        channel.ExchangeDeclare("media", type: ExchangeType.Fanout);
        channel.ExchangeDeclare("pb", type: ExchangeType.Fanout);

        publisher.Initialize(channel);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        channel.Close();
        connection.Close();

        return Task.CompletedTask;
    }
}
