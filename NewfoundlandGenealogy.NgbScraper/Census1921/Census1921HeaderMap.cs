using System.Text.RegularExpressions;
using FuzzySharp;

namespace NewfoundlandGenealogy.NgbScraper.Census1921;

public static class Census1921HeaderMap 
{
    private static readonly Dictionary<string,string> Exact = new(StringComparer.OrdinalIgnoreCase) {
        ["NAME"] = nameof(Census1921Record.Name),
        ["Surname"] = nameof(Census1921Record.Surname),
        ["Last Name"] = nameof(Census1921Record.Surname),
        ["Given"] = nameof(Census1921Record.GivenName),
        ["Given Name"] = nameof(Census1921Record.GivenName),
        ["Given Names"] = nameof(Census1921Record.GivenName),
        ["xCol. 3 Given Name"] = nameof(Census1921Record.GivenName),
        ["Christian Name"] = nameof(Census1921Record.GivenName),
        ["Sex"] = nameof(Census1921Record.Sex),
        ["M or F"] = nameof(Census1921Record.Sex),
        ["Relation"] = nameof(Census1921Record.RelationToHead),
        ["Rel."] = nameof(Census1921Record.RelationToHead),
        ["Rel'n"] = nameof(Census1921Record.RelationToHead),
        ["Relationship"] = nameof(Census1921Record.RelationToHead),
        ["Relation to Head"] = nameof(Census1921Record.RelationToHead),
        ["R'SHIP"] = nameof(Census1921Record.RelationToHead),
        ["Marital Status"] = nameof(Census1921Record.MaritalStatus),
        ["Status"] = nameof(Census1921Record.MaritalStatus),
        ["Cond."] = nameof(Census1921Record.MaritalStatus),
        ["Stat."] = nameof(Census1921Record.MaritalStatus),
        ["BIRTH DATE"] = nameof(Census1921Record.BirthDate),
        ["Birth"] = nameof(Census1921Record.BirthDate),
        ["Birth Year"] = nameof(Census1921Record.BirthYear),
        ["Year Born"] = nameof(Census1921Record.BirthYear),
        ["YOB"] = nameof(Census1921Record.BirthYear),
        ["YEAR OF BIRTH"] = nameof(Census1921Record.BirthYear),
        ["Birth Month"] = nameof(Census1921Record.BirthMonth),
        ["Month Born"] = nameof(Census1921Record.BirthMonth),
        ["MONTH OF BIRTH"] = nameof(Census1921Record.BirthMonth),
        ["Age"] = nameof(Census1921Record.Age),
        ["Age at last Birthday"] = nameof(Census1921Record.Age),
        ["Birth Place"] = nameof(Census1921Record.BirthPlace),
        ["Where Born"]  = nameof(Census1921Record.BirthPlace), 
        ["Place of Birth"] = nameof(Census1921Record.BirthPlace),
        ["P.O.B."] = nameof(Census1921Record.BirthPlace),
        ["Birth Pl."] = nameof(Census1921Record.BirthPlace),
        ["Birth Community"] = nameof(Census1921Record.BirthPlace),
        ["TOWN OF BIRTH"] = nameof(Census1921Record.BirthPlace),
        ["PL.BIRTH"] = nameof(Census1921Record.BirthPlace),
        ["Dwelling"] = nameof(Census1921Record.DwellingNumber),
        ["Dwelling Number"] = nameof(Census1921Record.DwellingNumber),
        ["Dwelling House"] = nameof(Census1921Record.DwellingNumber),
        ["House"] = nameof(Census1921Record.DwellingNumber),
        ["House Number"] = nameof(Census1921Record.DwellingNumber),
        ["Family"] = nameof(Census1921Record.FamilyNumber),
        ["Family Number"] = nameof(Census1921Record.FamilyNumber),
        ["Family Household"] = nameof(Census1921Record.FamilyNumber),
        ["Religion"] = nameof(Census1921Record.Religion),
        ["Address"] = nameof(Census1921Record.Residence),
        ["Living at"] = nameof(Census1921Record.Residence),
        ["Residence"] = nameof(Census1921Record.Residence),
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