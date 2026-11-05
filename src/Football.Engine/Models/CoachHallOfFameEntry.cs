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
}
