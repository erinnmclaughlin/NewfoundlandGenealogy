using NewfoundlandGenealogy.NgbScraper;
using NewfoundlandGenealogy.NgbScraper.Census1921;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddTransient<INgbScraper, Census1921Scraper>();

var host = builder.Build();
host.Run();