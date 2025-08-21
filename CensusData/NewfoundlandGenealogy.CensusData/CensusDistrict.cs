namespace NewfoundlandGenealogy.CensusData;

public sealed class CensusDistrict
{
    public required string Id { get; init; }
    public required string CensusId { get; set; }
    public required string DisplayName { get; set; }
    public required string NgbUrl { get; set; }
    public string? Notes { get; set; }

    public List<CensusTranscription> Transcriptions { get; set; } = [];

    public static string GetIdFromUrl(string url) => url.Split('/').Last().Split('.').First();
}
