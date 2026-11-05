using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Retains factual story threads without generating outcomes, quotes or interviews.</summary>
public static class FootballNarrativeMemoryService
{
    public static IReadOnlyList<FootballNarrativeMemoryRecord> Refresh(
        IReadOnlyList<FootballNarrativeMemoryRecord> current,
        IReadOnlyList<FootballNewsStory> stories,
        int seasonYear,
        IReadOnlyList<FootballNarrativeMemoryRecord>? extra = null)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(stories);
        var memories = current.ToDictionary(record => record.Id, StringComparer.Ordinal);
        var grouped = stories.Where(story => story.SeasonYear == seasonYear && !story.IsSocial &&
                !string.IsNullOrWhiteSpace(story.Id) && !string.IsNullOrWhiteSpace(story.TeamName))
            .GroupBy(story => (story.TeamName, Topic: NormalizeTopic(story.Category)));

        foreach (var group in grouped)
        {
            var ordered = group.OrderByDescending(story => story.Week)
                .ThenBy(story => story.Id, StringComparer.Ordinal).ToArray();
            var latest = ordered[0];
            var id = StableId(seasonYear, group.Key.TeamName, group.Key.Topic);
            memories.TryGetValue(id, out var existing);
            var sourceIds = (existing?.SourceStoryIds ?? [])
                .Concat(ordered.Select(story => story.Id))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var weeks = ordered.Select(story => story.Week).Where(week => week > 0).ToArray();
            memories[id] = new FootballNarrativeMemoryRecord
            {
                Id = id,
                TeamName = group.Key.TeamName,
                Topic = group.Key.Topic,
                Kind = NarrativeKind(group.Key.Topic),
                SeasonYear = seasonYear,
                FirstWeek = existing?.FirstWeek > 0 ? Math.Min(existing.FirstWeek, weeks.DefaultIfEmpty(existing.FirstWeek).Min()) : weeks.DefaultIfEmpty(0).Min(),
                LastWeek = Math.Max(existing?.LastWeek ?? 0, weeks.DefaultIfEmpty(latest.Week).Max()),
                LatestHeadline = latest.Headline,
                SourceStoryIds = sourceIds
            };
        }

        foreach (var record in extra ?? [])
            memories[record.Id] = record;

        return memories.Values
            .OrderByDescending(record => record.SeasonYear)
            .ThenByDescending(record => record.LastWeek)
            .ThenBy(record => record.Id, StringComparer.Ordinal)
            .Take(160)
            .ToArray();
    }

    private static string NormalizeTopic(string category) => category.Trim().ToLowerInvariant() switch
    {
        "injury" or "injuries" => "Health",
        "coach" or "coaches" or "coaching" => "Coaching",
        "award" or "awards" or "all-stars" or "all-star" => "Awards",
        "recruit" or "recruiting" or "recruiting result" => "Recruiting",
        "trade" or "trades" or "transaction" or "transactions" => "Transactions",
        "rivalry" or "rivalries" => "Rivalry",
        "" => "Team news",
        _ => category
    };

    private static FootballNarrativeKind NarrativeKind(string topic) => topic switch
    {
        "Health" or "Awards" => FootballNarrativeKind.Player,
        "Coaching" => FootballNarrativeKind.Coach,
        "Rivalry" => FootballNarrativeKind.Rivalry,
        _ => FootballNarrativeKind.Team
    };

    private static string StableId(int seasonYear, string teamName, string topic)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{seasonYear}:{teamName}:{topic}"));
        return "thread/" + Convert.ToHexString(bytes.AsSpan(0, 8)).ToLowerInvariant();
    }
}
