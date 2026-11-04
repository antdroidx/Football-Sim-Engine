using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>College-only historical rivalries. These do not influence simulation or schedule generation.</summary>
public static class TraditionalCollegeRivalries
{
    public static IReadOnlyList<(string TeamA, string TeamB, bool Annual)> Pairs { get; } =
    [
        ("Notre Dame", "USC", true),
        ("Notre Dame", "Stanford", true),

        ("Alabama", "Auburn", true),
        ("Ohio State", "Michigan", true),
        ("Oklahoma", "Texas", true),
        ("Florida", "Georgia", true),
        ("Harvard", "Yale", true),
        ("Texas", "Texas AM", true),
        ("Georgia", "Georgia Tech", true),
        ("Purdue", "Indiana", false),

        ("Michigan State", "Michigan", false),
        ("Nebraska", "Iowa", false),
        ("North Carolina", "Duke", false),
        ("Washington", "Washington State", false),
        ("Oregon", "Oregon State", false),
        ("South Carolina", "Clemson", false),
        ("Virginia", "Virginia Tech", false),
        ("Boston College", "Syracuse", false),
        ("NC State", "Wake Forest", false),

        ("Miami", "Florida State", false),
        ("Central Florida", "South Florida", false),
        ("Cincinnati", "Louisville", false),
        ("Houston", "Southern Methodist", false),
        ("Air Force", "Army", true),
        ("Air Force", "Navy", true),
        ("Army", "Navy", true),
        ("Memphis", "UAB", false),
        ("Toledo", "Bowling Green", false),

        ("Boise State", "Fresno State", false),
        ("UCLA", "Southern Methodist", false),
        ("California", "Stanford", false),
        ("Utah", "BYU", false),
        ("Northwestern", "Illinois", false),
        ("Minnesota", "Iowa", false),
        ("Maryland", "Penn State", false),
        ("Rutgers", "Penn State", false),
        ("UConn", "Notre Dame", false),
        ("Mississippi State", "Ole Miss", false),
        ("Arkansas", "Mississippi State", false),
        ("Auburn", "Georgia", false),
        ("Kentucky", "Louisville", false),
        ("Missouri", "Arkansas", false),
        ("Tennessee", "Vanderbilt", false),
        ("West Virginia", "Pittsburgh", false),
        ("Colorado", "Colorado State", false),
        ("New Mexico", "UNLV", false),
        ("Wyoming", "Utah", false),
        ("Hawaii", "San Diego State", false),
        ("San Jose State", "Fresno State", false),
        ("San Diego State", "Fresno State", false)
    ];

    public static bool IsTraditional(IReadOnlyList<Team> teams, string teamAName, string teamBName)
    {
        var leagueNames = teams.Select(team => team.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!leagueNames.Contains(teamAName) || !leagueNames.Contains(teamBName))
            return false;

        return Pairs.Any(pair =>
            pair.TeamA.Equals(teamAName, StringComparison.OrdinalIgnoreCase) &&
            pair.TeamB.Equals(teamBName, StringComparison.OrdinalIgnoreCase) ||
            pair.TeamA.Equals(teamBName, StringComparison.OrdinalIgnoreCase) &&
            pair.TeamB.Equals(teamAName, StringComparison.OrdinalIgnoreCase));
    }
}
