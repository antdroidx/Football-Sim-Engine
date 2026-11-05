using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Builds explicitly fictional in-game social posts from recorded stories.</summary>
public static class FootballNewsSocialService
{
    private static readonly (string Name, string Handle)[] MediaDesks =
    [
        ("Dynasty Wire", "@DynastyWire"),
        ("Gridiron Pulse", "@GridironPulse"),
        ("League Sideline", "@LeagueSideline")
    ];

    public static IReadOnlyList<FootballNewsStory> Build(IReadOnlyList<FootballNewsStory> stories,
        IReadOnlyList<FootballNarrativeMemoryRecord>? memories = null)
    {
        ArgumentNullException.ThrowIfNull(stories);
        var posts = stories.Where(story =>
            story.MediaKind is FootballNewsMediaKind.Story &&
            !string.IsNullOrWhiteSpace(story.Id))
            .Select(story =>
            {
                var seed = story.Id.Aggregate(story.TeamName.Length, (total, character) => total + character);
                var desk = MediaDesks[Math.Abs(seed) % MediaDesks.Length];
                var likes = 12 + Math.Abs(seed * 17) % 89;
                return new FootballNewsStory
                {
                    Id = "social/" + story.Id,
                    Category = "In-game social",
                    Headline = story.Headline,
                    Body = $"{story.TeamName} update · {story.Body}",
                    TeamName = story.TeamName,
                    SeasonYear = story.SeasonYear,
                    Week = story.Week,
                    RelatedId = story.Id,
                    AuthorName = desk.Name,
                    AuthorHandle = desk.Handle,
                    LikeCount = likes,
                    RepostCount = likes / 7,
                    ReplyCount = Math.Abs(seed * 13) % 18,
                    MediaKind = FootballNewsMediaKind.Social
                };
            })
            .ToList();
        foreach (var memory in memories?.Where(record => record.SourceStoryIds.Count >= 2) ?? [])
        {
            var source = stories.Where(story => memory.SourceStoryIds.Contains(story.Id))
                .OrderByDescending(story => story.Week).ThenBy(story => story.Id, StringComparer.Ordinal).FirstOrDefault();
            if (source is null || source.IsSocial) continue;
            var seed = memory.Id.Aggregate(memory.TeamName.Length, (total, character) => total + character);
            var desk = MediaDesks[Math.Abs(seed) % MediaDesks.Length];
            var likes = 18 + Math.Abs(seed * 19) % 113;
            posts.Add(new FootballNewsStory
            {
                Id = "social/" + memory.Id,
                Category = "Season thread",
                Headline = $"{memory.TeamName} · {memory.Topic} storyline",
                Body = $"Recorded {memory.SeasonYear} updates · Latest: {memory.LatestHeadline}",
                TeamName = memory.TeamName,
                SeasonYear = memory.SeasonYear,
                Week = memory.LastWeek,
                RelatedId = source.Id,
                AuthorName = desk.Name,
                AuthorHandle = desk.Handle,
                LikeCount = likes,
                RepostCount = likes / 8,
                ReplyCount = Math.Abs(seed * 11) % 16,
                MediaKind = FootballNewsMediaKind.Social
            });
        }
        return posts.DistinctBy(post => post.Id).ToArray();
    }
}
