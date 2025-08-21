using Microsoft.Extensions.Primitives;

namespace NewfoundlandGenealogy.NgbScraper.Census1921;

public sealed record Census1921Record
{
    public string DateReported { get; set; } = "";
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
    public string Employee { get; set; } = "";
    public string Employer { get; set; } = "";
    public string Occupation { get; set; } = "";
    public string SecondaryOccupation { get; set; } = "";
    public string Industry { get; set; } = "";
    public string Notes { get; set; } = "";
    public Dictionary<string, StringValues> AdditionalInformation { get; set; } = [];
}