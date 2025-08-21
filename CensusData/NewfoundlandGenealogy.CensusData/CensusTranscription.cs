namespace NewfoundlandGenealogy.CensusData;

public sealed class CensusTranscription
{
    public required string Id { get; set; }
    public required string CensusId { get; set; }
    public required string DistrictId { get; set; }
    public required string DisplayName { get; set; }
    public required string NgbUrl { get; set; }
    public required string MarkdownContent { get; set; }
    public string? Notes { get; set; }
    
    public static string GetIdFromUrl(string url) => url.Split('/').Last().Split('.').First();
    //public List<CensusDwellingGroup> DwellingGroups { get; set; } = [];
    //public List<RawCensusRecord> RawCensusRecords { get; set; } = [];
}