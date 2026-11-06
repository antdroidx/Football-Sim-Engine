using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>
/// The player's own activity on the in-game social feed. The engagement counts on a post are derived and stay
/// decorative; these records hold what the player actually did, and they survive a save.
/// </summary>
public static class FootballSocialService
{
    private const int MaximumReplyLength = 280;

    /// <summary>Marks the player's own likes, reposts, replies and followed desks on the derived posts.</summary>
    public static IReadOnlyList<FootballNewsStory> Apply(
        IReadOnlyList<FootballNewsStory> posts,
        IReadOnlyList<FootballSocialInteraction> interactions,
        IReadOnlyList<FootballSocialFollow> follows)
    {
        ArgumentNullException.ThrowIfNull(posts);
        ArgumentNullException.ThrowIfNull(interactions);
        ArgumentNullException.ThrowIfNull(follows);

        var liked = interactions
            .Where(item => item.Kind == FootballSocialInteractionKind.Like)
            .Select(item => item.PostId)
            .ToHashSet(StringComparer.Ordinal);
        var reposted = interactions
            .Where(item => item.Kind == FootballSocialInteractionKind.Repost)
            .Select(item => item.PostId)
            .ToHashSet(StringComparer.Ordinal);
        var replied = interactions
            .Where(item => item.Kind == FootballSocialInteractionKind.Reply && !string.IsNullOrWhiteSpace(item.Body))
            .Select(item => item.PostId)
            .ToHashSet(StringComparer.Ordinal);
        var followed = follows.Select(item => item.Handle).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return posts
            .Select(post => post with
            {
                LikedByPlayer = liked.Contains(post.Id),
                RepostedByPlayer = reposted.Contains(post.Id),
                RepliedByPlayer = replied.Contains(post.Id),
                FollowedAuthor = post.AuthorHandle is not null && followed.Contains(post.AuthorHandle)
            })
            .OrderByDescending(post => post.FollowedAuthor)
            .ThenByDescending(post => post.SeasonYear)
            .ThenByDescending(post => post.Week)
            .ToArray();
    }

    /// <summary>Records or clears one of the player's binary actions on a post.</summary>
    public static IReadOnlyList<FootballSocialInteraction> Toggle(
        IReadOnlyList<FootballSocialInteraction> current,
        string postId,
        FootballSocialInteractionKind kind,
        int seasonYear,
        int week)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (kind == FootballSocialInteractionKind.Reply)
            throw new ArgumentException("Use Reply to author or replace a reply.", nameof(kind));
        if (string.IsNullOrWhiteSpace(postId))
            return current;

        var id = $"{postId}:{kind}";
        return current.Any(item => item.Id == id)
            ? current.Where(item => item.Id != id).ToArray()
            : current.Append(new FootballSocialInteraction
            {
                Id = id,
                PostId = postId,
                Kind = kind,
                SeasonYear = seasonYear,
                Week = week
            }).ToArray();
    }

    /// <summary>Authors or replaces the player's reply to a post. Empty text removes it.</summary>
    public static IReadOnlyList<FootballSocialInteraction> Reply(
        IReadOnlyList<FootballSocialInteraction> current,
        string postId,
        string? body,
        int seasonYear,
        int week)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (string.IsNullOrWhiteSpace(postId))
            return current;

        var id = $"{postId}:{FootballSocialInteractionKind.Reply}";
        var without = current.Where(item => item.Id != id).ToArray();
        var text = (body ?? string.Empty).Trim();
        if (text.Length == 0)
            return without;
        return without.Append(new FootballSocialInteraction
        {
            Id = id,
            PostId = postId,
            Kind = FootballSocialInteractionKind.Reply,
            Body = text.Length <= MaximumReplyLength ? text : text[..MaximumReplyLength],
            SeasonYear = seasonYear,
            Week = week
        }).ToArray();
    }

    /// <summary>Follows or unfollows a media desk.</summary>
    public static IReadOnlyList<FootballSocialFollow> ToggleFollow(
        IReadOnlyList<FootballSocialFollow> current,
        string handle,
        string name,
        int seasonYear)
    {
        ArgumentNullException.ThrowIfNull(current);
        if (string.IsNullOrWhiteSpace(handle))
            return current;

        var id = handle.Trim().ToLowerInvariant();
        return current.Any(item => item.Id == id)
            ? current.Where(item => item.Id != id).ToArray()
            : current.Append(new FootballSocialFollow
            {
                Id = id,
                Handle = handle.Trim(),
                Name = name,
                SeasonYear = seasonYear
            }).ToArray();
    }

    /// <summary>
    /// Builds generated media-desk replies and the player's own saved reply. Generated replies restate a recorded
    /// category; player replies are authored text stored separately in the save.
    /// </summary>
    public static IReadOnlyList<FootballNewsStory> BuildReplies(
        IReadOnlyList<FootballNewsStory> posts,
        IReadOnlyList<FootballNewsStory> sources,
        IReadOnlyList<FootballSocialInteraction> interactions,
        int seasonYear,
        int week) =>
        ComposeReplies(posts, sources, seasonYear, week)
            .Concat(PlayerReplies(posts, interactions))
            .ToArray();

    /// <summary>
    /// Builds authored replies from the recorded story behind each post. A reply restates a recorded category and
    /// never invents a result, a quote or a statistic.
    /// </summary>
    public static IReadOnlyList<FootballNewsStory> ComposeReplies(
        IReadOnlyList<FootballNewsStory> posts,
        IReadOnlyList<FootballNewsStory> sources,
        int seasonYear,
        int week)
    {
        ArgumentNullException.ThrowIfNull(posts);
        ArgumentNullException.ThrowIfNull(sources);

        var replies = new List<FootballNewsStory>();
        foreach (var post in posts.Where(post => post.IsSocialPost))
        {
            var desks = FootballNewsSocialService.MediaDesks
                .Where(candidate => !string.Equals(candidate.Handle, post.AuthorHandle, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (desks.Length == 0)
                continue;
            var desk = desks[0];

            var category = sources.FirstOrDefault(source => source.Id == post.RelatedId)?.Category;
            replies.Add(new FootballNewsStory
            {
                Id = $"reply/{post.Id}",
                Category = "In-game reply",
                Headline = $"{post.TeamName} · reply",
                Body = ReplyBody(post.TeamName, category),
                TeamName = post.TeamName,
                SeasonYear = seasonYear,
                Week = week,
                RelatedId = post.Id,
                AuthorName = desk.Name,
                AuthorHandle = desk.Handle,
                MediaKind = FootballNewsMediaKind.SocialReply
            });
        }

        return replies;
    }

    public static IReadOnlyList<FootballNewsStory> PlayerReplies(
        IReadOnlyList<FootballNewsStory> posts,
        IReadOnlyList<FootballSocialInteraction> interactions)
    {
        ArgumentNullException.ThrowIfNull(posts);
        ArgumentNullException.ThrowIfNull(interactions);

        var postById = posts.Where(post => post.IsSocialPost)
            .ToDictionary(post => post.Id, StringComparer.Ordinal);
        return interactions
            .Where(item => item.Kind == FootballSocialInteractionKind.Reply &&
                           !string.IsNullOrWhiteSpace(item.Body) &&
                           postById.ContainsKey(item.PostId))
            .Select(item =>
            {
                var post = postById[item.PostId];
                return new FootballNewsStory
                {
                    Id = $"player-reply/{item.Id}",
                    Category = "Your reply",
                    Headline = $"{post.TeamName} · your reply",
                    Body = item.Body!,
                    TeamName = post.TeamName,
                    SeasonYear = item.SeasonYear,
                    Week = item.Week,
                    RelatedId = post.Id,
                    AuthorName = "You",
                    MediaKind = FootballNewsMediaKind.SocialReply,
                    AuthoredByPlayer = true
                };
            })
            .ToArray();
    }

    public static IReadOnlyList<FootballNewsStory> RepliesFor(
        IReadOnlyList<FootballNewsStory> replies,
        string postId) => replies.Where(reply => reply.RelatedId == postId).ToArray();

    private static string ReplyBody(string teamName, string? category) => category switch
    {
        "Injuries" => "Injury report noted.",
        "Recruiting" or "Recruiting Results" => "Recruiting board updated.",
        "Coaching" => "Staff update logged.",
        "Awards" or "Playoffs" or "Championships" => "Honors list updated.",
        "Transactions" or "Trades" => "Transaction log updated.",
        _ => $"{teamName} story logged."
    };
}
