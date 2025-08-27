using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NewfoundlandGenealogy.CensusData.Utils;

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
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(dbContext, RunMigrationsAsync, cancellationToken);
        }
        catch (Exception ex)
        { 
            activity?.AddException(ex);
            throw;
        }

        _appLifetime.StopApplication();
    }
    
    private static async Task RunMigrationsAsync(CensusDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        
        // todo: do something smarter here
        var transcriptions = await dbContext.CensusTranscriptions.ToListAsync(cancellationToken);
        
        foreach (var transcription in transcriptions.Where(x => x.MarkdownContent.Contains('`')))
        {
            transcription.MarkdownContent = transcription.MarkdownContent.Replace("`", "");
            transcription.ColumnNames = MarkdownUtils.EnumerateTableHeadersInFirstTable(transcription.MarkdownContent).Distinct().ToList();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
}