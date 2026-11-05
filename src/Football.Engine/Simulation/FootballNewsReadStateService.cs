using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class FootballNewsReadStateService
{
    public static IReadOnlyList<FootballNewsStory> ToggleSaved(IReadOnlyList<FootballNewsStory> current,
        FootballNewsStory story)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(story);
        return current.Any(item => item.Id == story.Id)
            ? current.Where(item => item.Id != story.Id).ToArray()
            : current.Append(story).ToArray();
    }

    public static IReadOnlyList<string> MarkRead(IReadOnlyList<string> current, IEnumerable<string> added)
    {
        ArgumentNullException.ThrowIfNull(current);
        return current.Concat(added)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<FootballNewsStory> Unread(IEnumerable<FootballNewsStory> stories,
        IReadOnlyList<string> readIds)
    {
        ArgumentNullException.ThrowIfNull(stories);
        return stories.Where(story => !readIds.Contains(story.Id)).ToArray();
    }
}
