using FuzzySharp;

namespace NewfoundlandGenealogy.NgbScraper.Census1921;

public static class Census1921HeaderMap 
{
    private static readonly Dictionary<string,string> Exact = new(StringComparer.OrdinalIgnoreCase) {
        ["Surname"] = "Surname", ["SurName"]="Surname", ["Last Name"]="Surname",
        ["Given Name"] = "GivenName", ["Given Names"]="GivenName", ["Christian Name"]="GivenName",
        ["Sex"] = "Sex", ["M or F"]="Sex",
        ["Relation"] = "Relation", ["Rel."]="Relation", ["Rel'n"]="Relation", ["Relationship"]="Relation",
        ["Marital Status"] = "MaritalStatus", ["Status"]="MaritalStatus", ["Cond."]="MaritalStatus",
        ["Birth Year"] = "BirthYear", ["Year Born"]="BirthYear", ["YOB"]="BirthYear",
        ["Birth Month"] = "BirthMonth", ["Month Born"]="BirthMonth", ["Mo."]="BirthMonth",
        ["Age"] = "Age",
        ["Birth Place"] = "BirthPlace", ["Where Born"]="BirthPlace", ["P.O.B."]="BirthPlace",
        ["Dwelling"] = "DwellingNumber", ["House"]="DwellingNumber",
        ["Family"] = "FamilyNumber"
    };

    public static string? ToCanonical(string headerToken)
    {
        if (Exact.TryGetValue(headerToken.Trim(), out var c)) return c;
        // fuzzy fallback against keys:
        var best = Exact.Keys
            .Select(k => (k, score: Fuzz.Ratio(k.ToLowerInvariant(), headerToken.ToLowerInvariant())))
            .OrderByDescending(x => x.score)
            .FirstOrDefault();
        return best.score >= 80 ? Exact[best.k] : null;
    }
}