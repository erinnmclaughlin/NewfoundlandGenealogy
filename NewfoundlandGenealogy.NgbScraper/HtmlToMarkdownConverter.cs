namespace NewfoundlandGenealogy.NgbScraper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

public sealed class HtmlToMarkdownOptions
{
    /// <summary>
    /// Repeat merged-cell text across the expanded span. If false, fill spanned cells with empty strings.
    /// </summary>
    public bool RepeatTextAcrossSpans { get; set; } = true;

    /// <summary>
    /// Prefer thead rows as header. If there is no thead, use the first row that contains any &lt;th&gt;.
    /// </summary>
    public bool PreferThead { get; set; } = true;

    /// <summary>
    /// If no header is found, synthesize one like "Column 1", "Column 2", ...
    /// </summary>
    public bool SynthesizeHeaderWhenMissing { get; set; } = true;

    /// <summary>
    /// Try to detect per-column alignment via align/style attributes and render :---, ---:, :---:
    /// </summary>
    public bool DetectAlignment { get; set; } = true;

    /// <summary>
    /// Convert rich inline HTML inside cells to Markdown using ReverseMarkdown. If false, plain text is extracted.
    /// </summary>
    public bool UseReverseMarkdownForCellContent { get; set; } = true;

    /// <summary>
    /// Combine multiple header rows into a single header by joining with &lt;br&gt; per column.
    /// </summary>
    public bool CombineMultipleHeaderRows { get; set; } = true;

    /// <summary>
    /// Trims whitespace in cells.
    /// </summary>
    public bool TrimCellText { get; set; } = true;
    
    /// <summary>
    /// Custom function to transform the table header
    /// </summary>
    public Func<string, string>? TableHeaderTransformer { get; set; }
}

public static class HtmlToMarkdownConverter
{
    public static string ConvertDocument(string html, Action<HtmlToMarkdownOptions>? optionsAction = null)
    {
        var options = new HtmlToMarkdownOptions();
        optionsAction?.Invoke(options);
        return ConvertDocument(html, options);
    }
    
    public static string ConvertDocument(string html, HtmlToMarkdownOptions options)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html ?? string.Empty);

        // Optional: strip non-content/noise elements
        //StripNoise(doc);

        // Collect tables and replace each with a SAFE placeholder (no underscores, asterisks, angles, etc.)
        var tableNodes = doc.DocumentNode.SelectNodes("//table")
                          ?? new HtmlNodeCollection(doc.DocumentNode);

        var tableMd = new Dictionary<string, string>(tableNodes.Count);
        int i = 0;
        foreach (var t in tableNodes.ToList())
        {
            // Safe token that survives ReverseMarkdown unchanged
            var key = $"§§TABMD{i}§§";
            tableMd[key] = ConvertSingleTable(t, options);

            // Replace <table> node with a text-node placeholder
            var placeholderNode = doc.CreateTextNode(key);
            t.ParentNode.ReplaceChild(placeholderNode, t);
            i++;
        }

        // If you only want tables (tables-only mode), return them joined
        //if (!options.ConvertNonTableContent)
        //    return string.Join(options.TableSeparator, tableMd.Values.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

        // Convert remaining HTML (with placeholders) to Markdown
        var conv = new ReverseMarkdown.Converter(new ReverseMarkdown.Config
        {
            GithubFlavored = true,
            // Prefer dropping unknown tags rather than passing raw HTML through
            UnknownTags = ReverseMarkdown.Config.UnknownTagsOption.Bypass, // keep inner text, drop tag wrappers
            RemoveComments = true
        });

        var mixedMd = conv.Convert(doc.DocumentNode.OuterHtml);

        // Swap placeholders back to table Markdown, optionally padding with blank lines
        foreach (var (key, md) in tableMd)
        {
            //var block = options.PadTablesWithBlankLines ? $"\n\n{md}\n\n" : md;
            var block = $"\n\n{md}\n\n";
            mixedMd = mixedMd.Replace(key, block);
        }

        // Tidy extra blank lines
        while (mixedMd.Contains("\n\n\n")) mixedMd = mixedMd.Replace("\n\n\n", "\n\n");
        return mixedMd.Trim();
    }
    
    private static string ConvertSingleTable(HtmlNode table, HtmlToMarkdownOptions opt)
    {
        // Gather rows in document order with metadata (is header row?)
        var allRowNodes = new List<(HtmlNode Row, bool IsHeader)>();

        // thead rows
        var thead = table.SelectSingleNode("./thead");
        if (thead != null)
        {
            foreach (var tr in thead.SelectNodes(".//tr") ?? Enumerable.Empty<HtmlNode>())
                allRowNodes.Add((tr, true));
        }

        // tbody or direct tr children
        var tbodyRows = table.SelectNodes("./tbody//tr")
                       ?? table.SelectNodes("./tr")
                       ?? Enumerable.Empty<HtmlNode>();
        foreach (var tr in tbodyRows)
        {
            // If not already included from thead:
            if (!allRowNodes.Any(x => x.Row == tr))
            {
                bool hasTh = tr.SelectNodes("./th")?.Any() ?? false;
                allRowNodes.Add((tr, hasTh && !opt.PreferThead));
            }
        }

        // tfoot rows (never header)
        foreach (var tr in table.SelectNodes("./tfoot//tr") ?? Enumerable.Empty<HtmlNode>())
        {
            if (!allRowNodes.Any(x => x.Row == tr))
                allRowNodes.Add((tr, false));
        }

        if (allRowNodes.Count == 0) return string.Empty;

        // Build a grid by expanding rowspans/colspans.
        var grid = new Dictionary<(int r, int c), string>();
        var alignment = new Dictionary<int, string>(); // col index -> "left"|"center"|"right"
        var rowIsHeader = new Dictionary<int, bool>();
        int maxRow = -1, maxCol = -1;

        var converter = opt.UseReverseMarkdownForCellContent
            ? new ReverseMarkdown.Converter(new ReverseMarkdown.Config { GithubFlavored = true })
            : null;

        for (int r = 0; r < allRowNodes.Count; r++)
        {
            var (rowNode, isHeaderCandidate) = allRowNodes[r];
            rowIsHeader[r] = isHeaderCandidate; // may be refined if THEAD exists

            int c = 0;
            while (grid.ContainsKey((r, c))) c++;

            var cells = rowNode.SelectNodes("./th|./td") ?? new HtmlNodeCollection(rowNode);

            foreach (var cell in cells)
            {
                while (grid.ContainsKey((r, c))) c++;

                int colSpan = GetSpan(cell, "colspan");
                int rowSpan = GetSpan(cell, "rowspan");
                if (colSpan < 1) colSpan = 1;
                if (rowSpan < 1) rowSpan = 1;

                var text = ExtractCellText(cell, converter, opt.TrimCellText);

                var align = GetAlignment(cell);
                if (opt.DetectAlignment && !string.IsNullOrEmpty(align))
                {
                    for (int j = 0; j < colSpan; j++)
                    {
                        int colIndex = c + j;
                        if (!alignment.ContainsKey(colIndex))
                            alignment[colIndex] = align!;
                    }
                }

                for (int dr = 0; dr < rowSpan; dr++)
                {
                    for (int dc = 0; dc < colSpan; dc++)
                    {
                        var pos = (r + dr, c + dc);
                        if (!grid.ContainsKey(pos))
                        {
                            grid[pos] = (dr == 0 && dc == 0) || opt.RepeatTextAcrossSpans ? text : "";
                            maxRow = Math.Max(maxRow, r + dr);
                            maxCol = Math.Max(maxCol, c + dc);
                        }
                    }
                }

                c += colSpan;
            }
        }

        // If we had any explicit THEAD, mark only those rows as header.
        var theadNode = table.SelectSingleNode("./thead");
        if (theadNode != null)
        {
            var theadRows = theadNode.SelectNodes(".//tr")?.ToHashSet() ?? new HashSet<HtmlNode>();
            for (int i = 0; i < allRowNodes.Count; i++)
                rowIsHeader[i] = theadRows.Contains(allRowNodes[i].Row);
        }

        // --- FALLBACK: if no <th> anywhere, use the first row as header (if enabled) ---
        var headerRowIndexes = rowIsHeader.Where(kv => kv.Value).Select(kv => kv.Key).OrderBy(i => i).ToList();
        if (headerRowIndexes.Count == 0 && allRowNodes.Count > 0)
        {
            rowIsHeader[0] = true;
            headerRowIndexes = new List<int> { 0 };
        }
        // -------------------------------------------------------------------------------

        // Build header row
        var headerRows = rowIsHeader.Where(kv => kv.Value).Select(kv => kv.Key).OrderBy(i => i).ToList();
        string[] header;
        var colCount = maxCol + 1;

        if (headerRows.Count > 0)
        {
            header = new string[colCount];
            for (int c = 0; c < colCount; c++)
            {
                var pieces = new List<string>();
                foreach (var hr in headerRows)
                {
                    var val = grid.TryGetValue((hr, c), out var v) ? v : "";
                    if (!string.IsNullOrWhiteSpace(val)) pieces.Add(val);
                }
                
                var headerCellValue = EscapeForMarkdown(string.Join("<br>", pieces));
                
                if (opt.TableHeaderTransformer != null)
                    headerCellValue = opt.TableHeaderTransformer(headerCellValue);
                
                header[c] = headerCellValue;
            }
        }
        else if (opt.SynthesizeHeaderWhenMissing)
        {
            header = Enumerable.Range(1, colCount).Select(i => $"Column {i}").ToArray();
        }
        else
        {
            header = Enumerable.Range(1, colCount).Select(_ => " ").ToArray();
        }

        // Body rows: all rows not marked header
        var rows = Enumerable.Range(0, maxRow + 1)
            .Where(r => !rowIsHeader.GetValueOrDefault(r))
            .Select(r =>
            {
                var cells = new string[colCount];
                for (int c = 0; c < colCount; c++)
                {
                    var val = grid.TryGetValue((r, c), out var v) ? v : "";
                    cells[c] = EscapeForMarkdown(val);
                }
                return cells;
            })
            .ToList();

        // Alignment row
        var alignRow = Enumerable.Range(0, colCount)
            .Select(c =>
            {
                var a = alignment.TryGetValue(c, out var v) ? v : "left";
                return a switch
                {
                    "center" => ":---:",
                    "right"  => "---:",
                    _        => ":---"
                };
            })
            .ToArray();

        // Compose Markdown
        var sb = new StringBuilder();
        sb.AppendLine("| " + string.Join(" | ", header) + " |");
        sb.AppendLine("| " + string.Join(" | ", alignRow) + " |");
        foreach (var row in rows)
            sb.AppendLine("| " + string.Join(" | ", row) + " |");

        return sb.ToString().TrimEnd();
    }


    private static int GetSpan(HtmlNode cell, string attr)
        => int.TryParse(cell.GetAttributeValue(attr, "1"), out var n) ? n : 1;

    private static string GetAlignment(HtmlNode cell)
    {
        var alignAttr = cell.GetAttributeValue("align", null)?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(alignAttr))
        {
            if (alignAttr is "center" or "right" or "left") return alignAttr;
        }
        var style = cell.GetAttributeValue("style", null) ?? "";
        var idx = style.IndexOf("text-align", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var seg = style.Substring(idx).Split(';', '}')[0];
            var parts = seg.Split(':');
            if (parts.Length >= 2)
            {
                var v = parts[1].Trim().ToLowerInvariant();
                if (v.StartsWith("center")) return "center";
                if (v.StartsWith("right")) return "right";
                if (v.StartsWith("left")) return "left";
            }
        }
        return "left";
    }

    private static string ExtractCellText(HtmlNode cell, ReverseMarkdown.Converter? converter, bool trim)
    {
        // Convert inner HTML to Markdown (or plaintext), preserving intended line breaks:
        // Strategy: turn block boundaries and <br> into '\n', then later map '\n' to "<br>" for GFM tables.
        string InnerAsMarkdown(HtmlNode node)
        {
            if (converter != null)
            {
                // Let ReverseMarkdown handle inline markup; then normalize newlines.
                var md = converter.Convert(node.InnerHtml ?? string.Empty);
                return md;
            }
            else
            {
                // Plain text fallback, respecting <br> and simple block-level separators.
                var sb = new StringBuilder();
                Walk(node, sb);
                return sb.ToString();

                static void Walk(HtmlNode n, StringBuilder sb)
                {
                    switch (n.NodeType)
                    {
                        case HtmlNodeType.Text:
                            sb.Append(System.Net.WebUtility.HtmlDecode(n.InnerText));
                            break;

                        case HtmlNodeType.Element:
                            var name = n.Name.ToLowerInvariant();
                            if (name is "br")
                            {
                                sb.Append('\n');
                                return;
                            }

                            bool block =
                                name is "p" or "div" or "section" or "article" or "ul" or "ol" or "li" or "table" or "tr";

                            if (block && sb.Length > 0 && sb[^1] != '\n')
                                sb.Append('\n');

                            if (name == "li") sb.Append("• ");

                            foreach (var child in n.ChildNodes)
                                Walk(child, sb);

                            if (block) sb.Append('\n');
                            break;
                    }
                }
            }
        }

        var text = InnerAsMarkdown(cell);

        // Normalize whitespace
        text = text.Replace("\r", "");
        // Collapse 3+ newlines to max 2 to avoid huge gaps inside cells
        while (text.Contains("\n\n\n"))
            text = text.Replace("\n\n\n", "\n\n");

        if (trim) text = text.Trim();

        // In GFM tables, literal newlines break the row. Use <br> to represent line breaks inside cells.
        text = text.Replace("\n", "<br>");

        return text;
    }

    private static string EscapeForMarkdown(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        // Escape pipes to avoid breaking the table structure.
        s = s.Replace("\\", "\\\\");    // escape backslashes first
        s = s.Replace("|", "\\|");
        // Non-breaking space -> normal space
        s = s.Replace('\u00A0', ' ');
        // Trim trailing spaces which can cause "hard breaks" in some renderers
        s = s.Trim();
        return s;
    }
}
