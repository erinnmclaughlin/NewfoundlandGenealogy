namespace NewfoundlandGenealogy.CensusData;

public sealed class CensusDwellingGroup
{
    public required string DwellingNumber { get; set; }
    public string? Residence { get; set; }
    public int? PageNumber { get; set; }
    public string? Notes { get; set; }
    
    public List<CensusFamilyGroup> FamilyGroups { get; set; } = [];
}