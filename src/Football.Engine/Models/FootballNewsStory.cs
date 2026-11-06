namespace DynastyManager.Core.Models;

public enum FootballNewsMediaKind { Story, Social, SocialReply }
public enum FootballPersonKind { Player, Coach }

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
    public string? AuthorName { get; init; }
    public string? AuthorHandle { get; init; }
    public int LikeCount { get; init; }
    public int RepostCount { get; init; }
    public int ReplyCount { get; init; }
    public FootballNewsMediaKind MediaKind { get; init; } = FootballNewsMediaKind.Story;
    public FootballPersonKind? PersonKind { get; init; }
    public Guid? PersonId { get; init; }
    public string? PersonName { get; init; }

    /// <summary>The player's own activity on this post. The derived counts stay decorative.</summary>
    public bool LikedByPlayer { get; init; }
    public bool RepostedByPlayer { get; init; }
    public bool FollowedAuthor { get; init; }

    public bool IsSocial => MediaKind is FootballNewsMediaKind.Social or FootballNewsMediaKind.SocialReply;
    public bool IsSocialPost => MediaKind is FootballNewsMediaKind.Social;
}
