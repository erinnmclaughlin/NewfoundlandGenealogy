using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace NewfoundlandGenealogy.CensusData.MigrationService;

public sealed class Worker(
    IHostApplicationLifetime appLifetime, 
    IDbContextFactory<CensusDbContext> dbContextFactory
) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private readonly IHostApplicationLifetime _appLifetime = appLifetime;
    private readonly IDbContextFactory<CensusDbContext> _dbContextFactory = dbContextFactory;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Client);

        try
        {
            var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await RunMigrationsAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        _appLifetime.StopApplication();
    }
    
    private static async Task RunMigrationsAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(dbContext.Database.MigrateAsync, cancellationToken);
    }
}