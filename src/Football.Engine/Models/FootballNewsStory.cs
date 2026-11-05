namespace DynastyManager.Core.Models;

public enum FootballNewsMediaKind { Story, Social }

public sealed record FootballNewsStory
{
    public required string Id { get; init; }
    public required string Category { get; init; }
    public required string Headline { get; init; }
    public required string Body { get; init; }
    public required string TeamName { get; init; }
    public int SeasonYear { get; init; }
    public int Week { get; init; }
    public string? RelatedId { get; init; }
    public FootballNewsMediaKind MediaKind { get; init; } = FootballNewsMediaKind.Story;

    public bool IsSocial => MediaKind is FootballNewsMediaKind.Social;
}
