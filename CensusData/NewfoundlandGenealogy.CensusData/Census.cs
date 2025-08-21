namespace NewfoundlandGenealogy.CensusData;

public sealed class Census
{
    public required string Id { get; init; }
    public required string DisplayName { get; set; }
    public required string NgbUrl { get; set; }
    public required int CensusYear { get; set; }
    public string? Notes { get; set; }

    public List<CensusDistrict> Districts { get; set; } = [];
}