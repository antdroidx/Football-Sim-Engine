using System.Text.Json;

namespace DynastyManager.Core.Models;

/// <summary>League-neutral rivalry identity and recorded game history.</summary>
public sealed record RivalryRecord(
    string Id,
    string TeamAName,
    string TeamBName,
    IReadOnlyList<RivalryGameRecord> Games)
{
    public bool Involves(string teamName) =>
        TeamAName.Equals(teamName, StringComparison.OrdinalIgnoreCase) ||
        TeamBName.Equals(teamName, StringComparison.OrdinalIgnoreCase);

    public bool Involves(string teamAName, string teamBName) =>
        Involves(teamAName) && Involves(teamBName);
}

/// <summary>One scheduled or played game inside a rivalry series.</summary>
public sealed record RivalryGameRecord(
    int SeasonYear,
    int Week,
    string HomeTeamName,
    string AwayTeamName,
    string? WinnerTeamName,
    bool IsNeutralSite,
    string? BowlName);
