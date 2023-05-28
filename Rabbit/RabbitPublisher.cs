using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace TNRD.Zeepkist.GTR.Backend.Rabbit;

internal class RabbitPublisher : IRabbitPublisher
{
    private IModel? channel;

    public void Initialize(IModel channel)
    {
        this.channel = channel;
    }

    public void Publish(string exchange, object data)
    {
        if (channel == null)
            return;

        string json = JsonConvert.SerializeObject(data);
        byte[] body = Encoding.UTF8.GetBytes(json);
        channel.BasicPublish(exchange, string.Empty, null, body);
    }
}
