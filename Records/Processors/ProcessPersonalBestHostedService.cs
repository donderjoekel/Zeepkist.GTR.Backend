using System.Threading.Channels;
using TNRD.Zeepkist.GTR.Backend.PersonalBests;
using TNRD.Zeepkist.GTR.Backend.Records.Requests;

namespace TNRD.Zeepkist.GTR.Backend.Records.Processors;

public class ProcessPersonalBestHostedService : BackgroundService
{
    private readonly ILogger<ProcessPersonalBestHostedService> _logger;
    private readonly IServiceProvider _provider;
    private readonly Channel<ProcessPersonalBestRequest> _channel;

    public ProcessPersonalBestHostedService(ILogger<ProcessPersonalBestHostedService> logger, IServiceProvider provider,
        Channel<ProcessPersonalBestRequest> channel)
    {
        _logger = logger;
        _provider = provider;
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _channel.Reader.WaitToReadAsync(stoppingToken))
        {
            ProcessPersonalBestRequest request = await _channel.Reader.ReadAsync(stoppingToken);
            using IServiceScope scope = _provider.CreateScope();
            IServiceProvider provider = scope.ServiceProvider;
            IPersonalBestsService personalBestsService = provider.GetRequiredService<IPersonalBestsService>();
            personalBestsService.UpdatePersonalBest(request.Record);
        }
    }
}
