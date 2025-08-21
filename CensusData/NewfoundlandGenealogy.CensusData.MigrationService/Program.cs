using NewfoundlandGenealogy.CensusData;
using NewfoundlandGenealogy.CensusData.MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.AddServiceDefaults();
builder.AddCensusDbContext();

var host = builder.Build();
host.Run();