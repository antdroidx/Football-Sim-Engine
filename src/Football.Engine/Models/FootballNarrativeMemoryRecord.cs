namespace DynastyManager.Core.Models;

public enum FootballNarrativeKind { Team, Player, Coach, Rivalry, Season }

public sealed record FootballNarrativeMemoryRecord
{
    public required string Id { get; init; }
    public required string TeamName { get; init; }
    public required string Topic { get; init; }
    public FootballNarrativeKind Kind { get; init; } = FootballNarrativeKind.Team;
    public int SeasonYear { get; init; }
    public int FirstWeek { get; init; }
    public int LastWeek { get; init; }
    public required string LatestHeadline { get; init; }
    public IReadOnlyList<string> SourceStoryIds { get; init; } = [];
}
