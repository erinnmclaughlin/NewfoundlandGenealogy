namespace NewfoundlandGenealogy.NgbScraper;

public sealed class Worker : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IServiceProvider _serviceProvider;

    public Worker(IHostApplicationLifetime appLifetime, IServiceProvider serviceProvider)
    {
        _appLifetime = appLifetime;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var scrapers = scope.ServiceProvider.GetServices<INgbScraper>();
        await Task.WhenAll(scrapers.Select(x => x.ExecuteAsync(stoppingToken)));
        
        _appLifetime.StopApplication();
    }
}