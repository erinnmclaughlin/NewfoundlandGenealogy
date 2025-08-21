using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NewfoundlandGenealogy.CensusData;

public static class ServiceRegistration
{
    /// <summary>
    /// Adds the <see cref="CensusDbContext"/> to the service container.
    /// </summary>
    public static void AddCensusDbContext<T>(this T builder) where T : IHostApplicationBuilder
    {
        builder.Services.AddDbContextFactory<CensusDbContext>(o =>
        {
            o.UseNpgsql(builder.Configuration.GetConnectionString(CensusDatabaseDefaults.Identifier), b =>
            {
                b.MigrationsAssembly("NewfoundlandGenealogy.CensusData.MigrationService");
            });
            
            o.UseSnakeCaseNamingConvention();
        });
        
        builder.EnrichNpgsqlDbContext<CensusDbContext>();
    }
}