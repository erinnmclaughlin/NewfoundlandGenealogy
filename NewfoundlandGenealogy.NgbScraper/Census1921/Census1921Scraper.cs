using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace NewfoundlandGenealogy.NgbScraper.Census1921;

public sealed partial class Census1921Scraper : INgbScraper
{
    private const string BaseUrl = "https://ngb.chebucto.org";
    private const string IndexPageUrl = $"{BaseUrl}/C1921/121-dist-idx.shtml";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var dir = Directory.CreateDirectory(DateTime.Now.ToString("yyyyMMddHHmmss"));
        
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync(IndexPageUrl);

        foreach (var (district, districtUrl) in await GetAnchorTagsAsync(page))
        {
            if (districtUrl.Contains("21-policies"))
                continue;
            
            await page.GotoAsync(districtUrl);

            foreach (var (community, communityUrl) in await GetAnchorTagsAsync(page))
            {
                if (communityUrl.Contains("21-policies"))
                    continue;

                await page.GotoAsync(communityUrl);

                var html = await page.InnerHTMLAsync("body");
                html = SphiderIgnore().Replace(html, string.Empty);
                html = MenuContainerIgnore().Replace(html, string.Empty);

                var fileName = $"{district.Replace(" ", "-")}_{community.Replace(" ", "-")}.md";
                await using var fileStream = new FileStream(Path.Combine(dir.FullName, fileName), FileMode.Create);
                var md = HtmlToMarkdownConverter.ConvertDocument(html, o =>
                {
                    o.CombineMultipleHeaderRows = true;
                    o.RepeatTextAcrossSpans = true;
                    o.TrimCellText = true;
                    o.UseReverseMarkdownForCellContent = false;
                    o.PreferThead = false;
                    o.SynthesizeHeaderWhenMissing = false;
                    o.TableHeaderTransformer = x => Census1921HeaderMap.ToCanonical(x) ?? x;
                });
                
                await using var writer = new StreamWriter(fileStream);
                await writer.WriteAsync(
                    $"""
                    {md}
                    
                    [Main Page Source]({IndexPageUrl})
                    
                    [District Page Source]({districtUrl})
                    
                    [Community Page Source]({communityUrl})
                    """);
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
