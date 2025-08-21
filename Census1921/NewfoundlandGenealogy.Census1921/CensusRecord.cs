namespace NewfoundlandGenealogy.Census1921;

public sealed class CensusRecord
{
    public string DistrictName { get; set; } = "";
    public int BookNumber { get; set; }
    public int PageNumber { get; set; }
    public int LineNumber { get; set; }
    public int DwellingNumber { get; set; }
    public int FamilyNumber { get; set; }
    public string PersonName { get; set; } = "";
    public string TownOrSettlement { get; set; } = "";
    public string Sex { get; set; } = "";
    public string RelationshipToHeadOfFamily { get; set; } = "";
    public string MaritalStatus { get; set; } = "";
    public string BirthYear { get; set; } = "";
    public string BirthMonth { get; set; } = "";
    public string AgeAtLastBirthday  { get; set; } = "";
    public string TownAndCityOfBirth { get; set; } = "";
    public string YearOfImmigrationToNewfoundland { get; set; } = "";
    public string YearOfNaturalizationToNewfoundland { get; set; } = "";
    public string Nationality { get; set; } = "";
    public string Religion { get; set; } = "";
    public string IsMicmacIndian { get; set; } = "";
    public string PersonalOccupation { get; set; } = "";
    public string Industry { get; set; } = "";
    public string IsEmployer { get; set; } = "";
    public string IsEmployee { get; set; } = "";
    public string IsSelfEmployed  { get; set; } = "";
    public string OtherWork { get; set; } = "";
}