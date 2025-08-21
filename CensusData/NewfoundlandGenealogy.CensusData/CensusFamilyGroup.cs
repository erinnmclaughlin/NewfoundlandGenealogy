namespace NewfoundlandGenealogy.CensusData;

public sealed class CensusFamilyGroup
{
    public required int FamilyNumber { get; set; }
    public int? YearReported { get; set; }
    public int? MonthReported { get; set; }
    public int? DayReported { get; set; }
    
    public string? GivenName { get; set; }
    public string? Surname { get; set; }
    public string? Sex { get; set; }
    public string? RelationToHead { get; set; }
}