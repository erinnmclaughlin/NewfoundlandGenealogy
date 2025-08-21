using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace NewfoundlandGenealogy.NgbScraper.Census1921;

public sealed partial class Census1921Scraper : INgbScraper
{
    private const string BaseUrl = "https://ngb.chebucto.org";
    private const string IndexPageUrl = $"{BaseUrl}/C1921/121-dist-idx.shtml";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync(IndexPageUrl);

        foreach (var (district, districtUrl) in await GetAnchorTagsAsync(page))
        {
            if (districtUrl.Contains("21-policies"))
                continue;
            
            Console.WriteLine(district);
            
            await page.GotoAsync(districtUrl);

            foreach (var (community, communityUrl) in await GetAnchorTagsAsync(page))
            {
                if (communityUrl.Contains("21-policies"))
                    continue;
                
                Console.WriteLine($"* {community}");

                await page.GotoAsync(communityUrl);

                var tables = await page.Locator("table").AllAsync();

                foreach (var table in tables)
                {
                    var headerCells = await table.Locator("th").AllAsync();

                    if (!headerCells.Any())
                        continue;
                    
                    var headerCellLabels = new List<string>();
                    
                    foreach (var headerCell in headerCells)
                    {
                        var headerCellText = await headerCell.InnerTextAsync();
                        headerCellText = headerCellText.Replace("\r", "").Replace("\n", " ");
                        
                        while (headerCellText.Contains("  "))
                            headerCellText = headerCellText.Replace("  ", " ");
                        
                        headerCellText = ColumnLabelPattern().Replace(headerCellText, "");

                        if (headerCellText.StartsWith("Col. "))
                        {
                            headerCellText = headerCellText.Replace("Col. ", "");
                        }

                        headerCellText = headerCellText.Trim();
                        var canonicalText = Census1921HeaderMap.ToCanonical(headerCellText);
                        headerCellLabels.Add($"{headerCellText} ({canonicalText})");
                    }

                    var maxRows = 10;
                    var rows = await table.Locator("tr").AllAsync();
                    foreach (var row in rows)
                    {
                        var cells = await row.Locator("td").AllAsync();

                        if (cells.Count != headerCellLabels.Count)
                            continue;

                        if (maxRows-- == 0)
                            return;

                        for (var k = 0; k < cells.Count; k++)
                        {
                            Console.WriteLine($"{headerCellLabels[k]}: {await cells[k].InnerTextAsync()}");
                        }
                        
                        Console.WriteLine();
                    }

                    return;
                }
                
                return;
            }
            
            Console.WriteLine();
            await page.GoBackAsync();

            return;
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
}

public sealed record AnchorTagInfo(string DisplayName, string Href);
