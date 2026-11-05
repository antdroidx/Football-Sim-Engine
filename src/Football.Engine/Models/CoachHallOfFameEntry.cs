namespace DynastyManager.Core.Models;

public sealed record CoachHallOfFameEntry
{
    public Guid CoachId { get; init; }
    public required string CoachName { get; init; }
    public required string TeamName { get; init; }
    public int InductionYear { get; init; }
    public int HeadCoachSeasons { get; init; }
    public int HeadCoachWins { get; init; }
    public int HeadCoachLosses { get; init; }
    public int ConferenceTitles { get; init; }
    public int PlayoffAppearances { get; init; }
    public int NationalTitles { get; init; }
    public int CareerScore { get; init; }
    public int HonorsScore { get; init; }
    public required string Citation { get; init; }

    /// <summary>One line per team the coach led, with the seasons and the record at each stop.</summary>
    public string[] TeamCitations { get; init; } = [];
    public bool Retired { get; init; }
    public int? RetiredYear { get; init; }
}

/// <summary>One team stop in a coach's recorded head coaching career.</summary>
public sealed record CoachHallOfFameTeamSpan
{
    public required string TeamName { get; init; }
    public int FirstYear { get; init; }
    public int LastYear { get; init; }
    public int Seasons { get; init; }
    public int Wins { get; init; }
    public int Losses { get; init; }
    public string Citation => Seasons == 1
        ? $"{TeamName} {FirstYear} ({Wins}-{Losses})"
        : $"{TeamName} {FirstYear}-{LastYear} ({Wins}-{Losses})";
}