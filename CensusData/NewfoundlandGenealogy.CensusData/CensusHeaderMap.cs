using System.Text.RegularExpressions;
using FuzzySharp;

namespace NewfoundlandGenealogy.CensusData;

public static class CensusHeaderMap 
{
    private static readonly Dictionary<string,string> Exact = new(StringComparer.OrdinalIgnoreCase) {
        ["NAME"] = "Name",
        ["Surname"] = "Surname",
        ["Last Name"] = "Surname",
        ["Given"] = "GivenName",
        ["Given Name"] = "GivenName",
        ["Given Names"] = "GivenName",
        ["xCol. 3 Given Name"] = "GivenName",
        ["Christian Name"] = "GivenName",
        ["Sex"] = "Sex",
        ["M or F"] = "Sex",
        ["Relation"] = "RelationToHead",
        ["Rel."] = "RelationToHead",
        ["Rel'n"] = "RelationToHead",
        ["Relationship"] = "RelationToHead",
        ["Relation to Head"] = "RelationToHead",
        ["R'SHIP"] = "RelationToHead",
        ["Marital Status"] = "MaritalStatus",
        ["Status"] = "MaritalStatus",
        ["Cond."] = "MaritalStatus",
        ["Stat."] = "MaritalStatus",
        ["BIRTH DATE"] = "BirthDate",
        ["Birth"] = "BirthDate",
        ["Birth Year"] = "BirthYear",
        ["Year Born"] = "BirthYear",
        ["YOB"] = "BirthYear",
        ["YEAR OF BIRTH"] = "BirthYear",
        ["Birth Month"] = "BirthMonth",
        ["Month Born"] = "BirthMonth",
        ["MONTH OF BIRTH"] = "BirthMonth",
        ["Age"] = "Age",
        ["Age at last Birthday"] = "Age",
        ["Birth Place"] = "BirthPlace",
        ["Where Born"]  = "BirthPlace", 
        ["Place of Birth"] = "BirthPlace",
        ["P.O.B."] = "BirthPlace",
        ["Birth Pl."] = "BirthPlace",
        ["Birth Community"] = "BirthPlace",
        ["TOWN OF BIRTH"] = "BirthPlace",
        ["PL.BIRTH"] = "BirthPlace",
        ["Dwelling"] = "DwellingNumber",
        ["Dwelling Number"] = "DwellingNumber",
        ["Dwelling House"] = "DwellingNumber",
        ["House"] = "DwellingNumber",
        ["House Number"] = "DwellingNumber",
        ["Family"] = "FamilyNumber",
        ["Family Number"] = "FamilyNumber",
        ["Family Household"] = "FamilyNumber",
        ["Religion"] = "Religion",
        ["Address"] = "Residence",
        ["Living at"] = "Residence",
        ["Residence"] = "Residence",
        ["Year"] = "Year",
        ["YR."] = "Year",
        ["Mo."] = "Month",
        ["Month"] = "Month",
        ["Mon"] = "Month",
        ["DESCRIPTION"] = "Description",
        ["ADDITIONAL INFORMATION"] = "AdditionalInformation",
        ["Nationality"] = "Nationality",
        ["Occupation"] = "Occupation",
        ["OCCUPATION IF GIVEN"] = "Occupation",
        ["OCC."] = "Occupation",
        ["SECONDARY OCC"] = "SecondaryOccupation",
        ["RESEARCH NOTES"] = "Notes",
        ["NOTES"] = "Notes",
        ["Amalie Lewis Tuffin added notes"] = "Notes",
        ["Amalie Tuffin Notes"] = "Notes",
        ["Researcher's Notes"] = "Notes",
        ["Transcriber Notes: By Bill Crant"] = "Notes",
        ["Date"] = "Date"
        
    };

    public static IEnumerable<string> EnumerateExactHeaders() => Exact.Values.Distinct();

    public static string? ToCanonical(string headerToken)
    {
        if (headerToken.StartsWith("Col ") || headerToken.StartsWith("Col. "))
        {
            headerToken = Regex.Replace(headerToken, "Col.? \\d+", "");
        }
        
        if (Exact.TryGetValue(headerToken.Trim(), out var c)) return c;
        // fuzzy fallback against keys:
        var best = Exact.Keys
            .Select(k => (k, score: Fuzz.Ratio(k.ToLowerInvariant(), headerToken.ToLowerInvariant())))
            .OrderByDescending(x => x.score)
            .FirstOrDefault();
        
        return best.score >= 80 ? Exact[best.k] : null;
    }
}