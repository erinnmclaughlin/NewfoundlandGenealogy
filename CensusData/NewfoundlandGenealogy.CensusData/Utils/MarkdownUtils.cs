using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace NewfoundlandGenealogy.CensusData.Utils;

public static class MarkdownUtils
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();
    
    public static IEnumerable<string> EnumerateTableHeadersInFirstTable(string markdown)
    {
        // 1) Try Markdig with a bumped nesting limit (if the property exists)
        var md = TryParseWithHigherNesting(markdown, Pipeline);

        // 2) If Markdig still failed, try to isolate just the first table region and parse that
        if (md is null)
        {
            var snippet = SliceFirstPipeTableRegion(markdown, maxLines: 400); // tiny slice around the first table
            if (!string.IsNullOrEmpty(snippet))
                md = TryParseWithHigherNesting(snippet, Pipeline);
        }

        // 3) If we still don't have a doc, use the manual fallback to get header cells, then continue
        if (md is null)
        {
            foreach (var header in TryExtractFirstPipeHeaderManually(markdown))
                yield return CensusHeaderMap.ToCanonical(header) ?? header;
            
            yield break;
        }
        
        var firstTable = md.Descendants<Table>().FirstOrDefault();

        if (firstTable is null)
            yield break;
        
        // Prefer an explicit header row; if none, use the first row
        var headerRow = firstTable.Descendants<TableRow>().FirstOrDefault(r => r.IsHeader)
                        ?? firstTable.Descendants<TableRow>().FirstOrDefault();
        
        if (headerRow is null) 
            yield break;
        
        // Only direct child cells of this header row
        var headerCells = headerRow.Descendants<TableCell>().Where(c => ReferenceEquals(c.Parent, headerRow));

        foreach (var cell in headerCells)
        {
            var cellValue = GetCellPlainText(cell);
            if (string.IsNullOrWhiteSpace(cellValue)) continue;

            yield return CensusHeaderMap.ToCanonical(cellValue) ?? cellValue;
        }
    }
    
    static string GetCellPlainText(TableCell cell)
    {
        var sb = new StringBuilder();

        foreach (var inline in cell.Descendants<Inline>())
        {
            switch (inline)
            {
                case LiteralInline lit:
                    sb.Append(lit.Content.ToString());
                    break;

                case CodeInline code:
                    sb.Append(code.Content.ToString());
                    break;

                case HtmlEntityInline ent:
                    // Prefer decoded text if available
                    if (!string.IsNullOrEmpty(ent.Transcoded.Text))
                        sb.Append(ent.Transcoded);
                    else
                        sb.Append(ent.Span.ToString());
                    break;

                case LineBreakInline:
                    sb.Append(' ');
                    break;
            }
        }

        // Collapse whitespace and trim
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }

    static MarkdownDocument? TryParseWithHigherNesting(string text, MarkdownPipeline pipeline)
    {
        try
        {
            var ctx = new MarkdownParserContext();
            // Bump MaxNesting if this build exposes it (reflection keeps this source-compatible)
            var prop = ctx.GetType().GetProperty("MaxNesting");
            prop?.SetValue(ctx, 4096);
            return Markdown.Parse(text, pipeline, ctx);
        }
        catch (ArgumentException ex) when (ex.Message.IndexOf("depth", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return null; // depth limit hit
        }
        catch
        {
            return null;
        }
    }

    static string SliceFirstPipeTableRegion(string text, int maxLines)
    {
        // Grab a small window starting at the first line that looks like a pipe table header
        var lines = text.AsSpan();
        var reader = new StringReader(text);
        var buf = new List<string>(maxLines);
        string? line;
        bool inWindow = false;
        while ((line = reader.ReadLine()) is not null)
        {
            if (!inWindow)
            {
                // candidate: a `|` line followed by a delimiter row `| --- | :---: |`
                if (LooksLikeHeaderRow(line))
                {
                    var peek = reader.ReadLine();
                    if (peek is not null && LooksLikeDelimiterRow(peek))
                    {
                        inWindow = true;
                        buf.Add(line);
                        buf.Add(peek);
                        continue;
                    }
                }
            }
            else
            {
                buf.Add(line);
                if (buf.Count >= maxLines) break;
            }
        }
        return buf.Count == 0 ? "" : string.Join(Environment.NewLine, buf);

        static bool LooksLikeHeaderRow(string l) => l.Contains('|') && !Regex.IsMatch(l, @"^\s*<{3,}|^-{3,}");
        static bool LooksLikeDelimiterRow(string l) => Regex.IsMatch(l, @"^\s*\|?\s*:?-{3,}:?\s*(\|\s*:?-{3,}:?\s*)+\|?\s*$");
    }

    static IEnumerable<string> TryExtractFirstPipeHeaderManually(string text)
    {
        // Quick & dirty: find a header row + delimiter row and split the header by pipes
        var lines = text.Split('\n');
        for (int i = 0; i + 1 < lines.Length; i++)
        {
            var head = lines[i].TrimEnd('\r');
            var delim = lines[i + 1].TrimEnd('\r');
            if (!head.Contains('|')) continue;
            if (!Regex.IsMatch(delim, @"^\s*\|?\s*:?-{3,}:?\s*(\|\s*:?-{3,}:?\s*)+\|?\s*$")) continue;

            return head.Trim().Trim('|')
                       .Split('|')
                       .Select(s => Regex.Replace(s, @"\s+", " ").Trim())
                       .Where(s => s.Length > 0);
        }
        return Array.Empty<string>();
    }

    static string GetCellTextQuick(TableCell cell)
    {
        // Cells typically contain ParagraphBlock(s) with inlines; concatenate leaf inline contents.
        var sb = new StringBuilder();
        for (int i = 0; i < cell.Count; i++)
        {
            if (cell[i] is Markdig.Syntax.LeafBlock leaf && leaf.Inline is not null)
            {
                var inline = leaf.Inline.FirstChild;
                int steps = 0;
                while (inline is not null && steps++ < 100_000)
                {
                    switch (inline)
                    {
                        case Markdig.Syntax.Inlines.LiteralInline lit: sb.Append(lit.Content.ToString()); break;
                        case Markdig.Syntax.Inlines.CodeInline code: sb.Append(code.Content.ToString()); break;
                        case Markdig.Syntax.Inlines.HtmlEntityInline ent:
                            sb.Append(!string.IsNullOrEmpty(ent.Transcoded.Text) ? ent.Transcoded : ent.Span.ToString());
                            break;
                        case Markdig.Syntax.Inlines.LineBreakInline: sb.Append(' '); break;
                    }
                    inline = inline.NextSibling ?? (inline is Markdig.Syntax.Inlines.ContainerInline ci ? ci.FirstChild : null);
                }
                if (i < cell.Count - 1) sb.Append(' ');
            }
        }
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }
}