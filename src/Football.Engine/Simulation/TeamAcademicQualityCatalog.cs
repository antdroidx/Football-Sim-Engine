using DynastyManager.Core.Models;
namespace DynastyManager.Core.Simulation;

/// <summary>
/// Stable academic-quality estimates for recruiting and team profile display. Highly selective institutions
/// are explicit; other schools use a prestige-informed fallback until a reviewed institution dataset is added.
/// </summary>
public static class TeamAcademicQualityCatalog
{
    static readonly IReadOnlyDictionary<string,int> Selective = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase)
    {
        ["Stanford"]=98,["California"]=96,["UCLA"]=95,["Northwestern"]=95,["Duke"]=95,
        ["Rice"]=94,["Vanderbilt"]=94,["Notre Dame"]=93,["Michigan"]=92,["Virginia"]=92,
        ["North Carolina"]=90,["Boston College"]=90,["USC"]=89,["Wake Forest"]=89,
        ["Georgia Tech"]=88,["Tulane"]=88,["Southern Methodist"]=87,["BYU"]=86,
        ["Penn State"]=85,["Wisconsin"]=85,["Illinois"]=84,["Maryland"]=84,
        ["Pittsburgh"]=83,["Rutgers"]=83,["Colorado"]=82,["Washington"]=82,
        ["TCU"]=81,["Baylor"]=81,["Syracuse"]=80,["Miami"]=80,
        ["Florida"]=79,["Georgia"]=79,["Ohio State"]=79,["Texas"]=79,
        ["UConn"]=78,["Delaware"]=79,["William & Mary"]=94
    };

    public static int Get(Team team) => team.AcademicQuality >= 0
        ? Math.Clamp(team.AcademicQuality, 0, 99)
        : For(team.Name, team.Prestige);

    public static int For(string school, int prestige) => Selective.TryGetValue(school, out var rating)
        ? rating
        : Math.Clamp(48 + Math.Clamp(prestige,0,99) / 5, 45, 78);
}
