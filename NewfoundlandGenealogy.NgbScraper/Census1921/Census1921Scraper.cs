using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using NewfoundlandGenealogy.CensusData;

namespace NewfoundlandGenealogy.NgbScraper.Census1921;

public sealed partial class Census1921Scraper(IDbContextFactory<CensusDbContext> dbContextFactory, ILogger<Census1921Scraper> logger) : INgbScraper
{
    private const string CensusId = "C1921";
    private const string BaseUrl = "https://ngb.chebucto.org";
    private const string IndexPageUrl = $"{BaseUrl}/{CensusId}/121-dist-idx.shtml";

    private readonly IDbContextFactory<CensusDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<Census1921Scraper> _logger = logger;
    
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(dbContext, ScrapeCensusData, cancellationToken);
    }

    private async Task ScrapeCensusData(CensusDbContext dbContext, CancellationToken cancellationToken)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync(IndexPageUrl);

        if (!await dbContext.Censuses.AnyAsync(x => x.Id == CensusId, cancellationToken))
        {
            dbContext.Add(new Census
            {
                Id = CensusId,
                DisplayName = "1921 Newfoundland Population Census",
                NgbUrl = IndexPageUrl,
                CensusYear = 1921
            });
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        
        foreach (var (districtName, districtUrl) in await GetAnchorTagsAsync(page))
        {
            if (districtUrl.Contains("21-policies"))
                continue;
            
            await page.GotoAsync(districtUrl);
            
            var districtId = CensusDistrict.GetIdFromUrl(districtUrl);

            if (!await dbContext.CensusDistricts.AnyAsync(x => x.CensusId == CensusId && x.Id == districtId, cancellationToken))
            {
                dbContext.Add(new CensusDistrict
                {
                    Id = districtId,
                    CensusId = CensusId,
                    DisplayName = districtName,
                    NgbUrl = districtUrl
                });
                
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            
            var transcriptions = await dbContext.CensusTranscriptions
                .Where(x => x.CensusId == CensusId && x.DistrictId == districtId)
                .ToListAsync(cancellationToken);

            var newTranscriptionIds = new List<string>();
            
            foreach (var (transcriptionName, transcriptionUrl) in await GetAnchorTagsAsync(page))
            {
                if (transcriptionUrl.Contains("21-policies"))
                    continue;

                await page.GotoAsync(transcriptionUrl);

                var html = await page.InnerHTMLAsync("body");
                html = ColumnLabelPattern().Replace(html, string.Empty);
                html = SphiderIgnore().Replace(html, string.Empty);
                html = MenuContainerIgnore().Replace(html, string.Empty);

                //var fileName = $"{districtName.Replace(" ", "-")}_{transcriptionName.Replace(" ", "-")}.md";
                //await using var fileStream = new FileStream(Path.Combine(dir.FullName, fileName), FileMode.Create);
                var md = HtmlToMarkdownConverter.ConvertDocument(html, o =>
                {
                    o.CombineMultipleHeaderRows = true;
                    o.RepeatTextAcrossSpans = true;
                    o.TrimCellText = true;
                    o.UseReverseMarkdownForCellContent = false;
                    o.PreferThead = false;
                    o.SynthesizeHeaderWhenMissing = false;
                    o.TableHeaderTransformer = x => CensusHeaderMap.ToCanonical(x) ?? x;
                });
                
                var transcriptionId = CensusTranscription.GetIdFromUrl(transcriptionUrl);
                var transcription = transcriptions.FirstOrDefault(x => x.Id == transcriptionId);

                if (transcription == null)
                {
                    if (newTranscriptionIds.Contains(transcriptionId))
                    {
                        _logger.LogWarning("Found duplicate transcription ID ({TranscriptionId}) for district at URL {DistrictUrl}. Skipping.", transcriptionId, districtUrl);
                        continue;
                    }
                    
                    transcription = new CensusTranscription
                    {
                        Id = transcriptionId,
                        CensusId = CensusId,
                        DistrictId = districtId,
                        DisplayName = transcriptionName,
                        NgbUrl = transcriptionUrl,
                        MarkdownContent = md
                    };
                    dbContext.Add(transcription);
                    newTranscriptionIds.Add(transcriptionId);
                }
                else
                {
                    transcription.MarkdownContent = md;
                }
               
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            
            await page.GoBackAsync();
        }
    }

    private static async Task<List<AnchorTagInfo>> GetAnchorTagsAsync(IPage page)
    {
        var anchorTags = new List<AnchorTagInfo>();
        var currentUrl = page.Url[..page.Url.LastIndexOf('/')];
        
        foreach (var link in await page.Locator("body a").AllAsync())
        {
            var href = await link.GetAttributeAsync("href");

            if (href is null)
                continue;
            
            if (!href.StartsWith("http://") && !href.StartsWith("https://") && !href.StartsWith('/') && !href.StartsWith('.') && href.EndsWith(".shtml"))
            {
                var displayText = await link.InnerTextAsync();
                displayText = displayText.Replace("\r", "").Replace("\n", "");
                anchorTags.Add(new AnchorTagInfo(displayText, $"{currentUrl}/{href}"));
            }
        }

        return anchorTags; 
    }

    [GeneratedRegex(@"(?im)^(?:\s*[*•\-]?\s*)(?:(?:Col(?:umn)?\.?\s*)?\d+(?:[A-Za-z](?![A-Za-z]))?)(?:\s*[:.)-]?\s*)", RegexOptions.None, "en-US")]
    private static partial Regex ColumnLabelPattern();
    
    [GeneratedRegex(@"<!--sphider_noindex-->.*?<!--/sphider_noindex-->", RegexOptions.Singleline)]
    private static partial Regex SphiderIgnore();
    
    [GeneratedRegex(@"<div[^>]*class\s*=\s*[""']?mmenucontainer[""']?[^>]*>.*?</div>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex MenuContainerIgnore();
}

public sealed record AnchorTagInfo(string DisplayName, string Href);
