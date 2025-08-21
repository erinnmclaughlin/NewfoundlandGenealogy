using Microsoft.Extensions.Primitives;

namespace NewfoundlandGenealogy.NgbScraper.Census1921;

public sealed record CensusHousehold
{
    public int HouseholdNumber { get; set; }
    public string Residence { get; set; } = "";
    public string District { get; set; } = "";
    public string TownOrCommunity { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public List<CensusFamilyGroup> FamilyGroups { get; set; } = [];
    public Dictionary<string, StringValues> AdditionalInformation { get; set; } = [];
}

public sealed record CensusFamilyGroup
{
    public int FamilyNumber { get; init; }
    public List<CensusPerson> People { get; set; } = [];
    public Dictionary<string, StringValues> AdditionalInformation { get; set; } = [];
}

public sealed record CensusPerson
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public int PageNumber { get; init; }
    public string Name { get; set; } = "";
    public string GivenName { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Sex { get; set; } = "";
    public string RelationToHead { get; set; } = "";
    public string MaritalStatus { get; set; } = "";
    public string BirthDate { get; set; } = "";
    public string BirthYear { get; set; } = "";
    public string BirthMonth { get; set; } = "";
    public string Age { get; set; } = "";
    public string BirthPlace { get; set; } = "";
    public string DwellingNumber { get; set; } = "";
    public string FamilyNumber { get; set; } = "";
    public string Religion { get; set; } = "";
    public string Nationality { get; set; } = "";
    public string Occupation { get; set; } = "";
    public string SecondaryOccupation { get; set; } = "";
    public string Notes { get; set; } = "";
    public Dictionary<string, StringValues> AdditionalInformation { get; set; } = [];
}

public sealed record Census1921Record
{
    public string Name { get; set; } = "";
    public string GivenName { get; set; } = "";
    public string Surname { get; set; } = "";
    public string Sex { get; set; } = "";
    public string RelationToHead { get; set; } = "";
    public string MaritalStatus { get; set; } = "";
    public string BirthDate { get; set; } = "";
    public string BirthYear { get; set; } = "";
    public string BirthMonth { get; set; } = "";
    public string Age { get; set; } = "";
    public string BirthPlace { get; set; } = "";
    public string DwellingNumber { get; set; } = "";
    public string FamilyNumber { get; set; } = "";
    public string Religion { get; set; } = "";
    public string Residence { get; set; } = "";
    public string Nationality { get; set; } = "";
    public string Occupation { get; set; } = "";
    public string SecondaryOccupation { get; set; } = "";
    public string Notes { get; set; } = "";
    public Dictionary<string, StringValues> AdditionalInformation { get; set; } = [];
}