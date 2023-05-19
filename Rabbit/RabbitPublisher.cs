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

    public void Publish(PublishableRecord record)
    {
        if (channel == null)
            return;

        string json = JsonConvert.SerializeObject(record);
        byte[] body = Encoding.UTF8.GetBytes(json);
        channel.BasicPublish("records", string.Empty, null, body);
    }
}
