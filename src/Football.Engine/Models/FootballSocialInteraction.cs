namespace DynastyManager.Core.Models;

public enum FootballSocialInteractionKind { Like, Repost, Reply }

/// <summary>An action the player took on an in-game media post.</summary>
public sealed record FootballSocialInteraction
{
    public required string Id { get; init; }
    public required string PostId { get; init; }
    public required FootballSocialInteractionKind Kind { get; init; }
    public string? Body { get; init; }
    public int SeasonYear { get; init; }
    public int Week { get; init; }
}

/// <summary>A media desk the player follows.</summary>
public sealed record FootballSocialFollow
{
    public required string Id { get; init; }
    public required string Handle { get; init; }
    public required string Name { get; init; }
    public int SeasonYear { get; init; }
}
