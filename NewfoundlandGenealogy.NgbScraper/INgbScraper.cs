namespace NewfoundlandGenealogy.NgbScraper;

public interface INgbScraper
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}