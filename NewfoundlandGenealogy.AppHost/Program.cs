using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume().WithPgWeb();
var postgresDb = postgres.AddDatabase("census-database");

var censusDbMigrationService = builder
    .AddProject<NewfoundlandGenealogy_CensusData_MigrationService>("census-database-migration-service")
    .WithReference(postgresDb)
    .WaitFor(postgresDb);

var ngbScraper = builder
    .AddProject<NewfoundlandGenealogy_NgbScraper>("ngb-scraper")
    .WithReference(postgresDb)
    .WaitForCompletion(censusDbMigrationService)
    .WithExplicitStart();

builder.Build().Run();