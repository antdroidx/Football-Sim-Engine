using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;

namespace ProFootball.Core;

/// <summary>Prepares Pro news sources and persistent narrative memory away from the MAUI render loop.</summary>
public static class ProNewsPresentation
{
    public static IReadOnlyList<FootballNewsStory> Sources(ProFranchise state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.News.Select(story => new FootballNewsStory
        {
            Id = story.Id,
            Category = story.Category,
            Headline = story.Headline,
            Body = story.Body,
            TeamName = story.Team ?? state.UserTeam,
            SeasonYear = story.Year,
            Week = story.Week,
            RelatedId = story.GameId,
            MediaKind = FootballNewsMediaKind.Story,
            PersonKind = story.PersonKind,
            PersonId = story.PersonId,
            PersonName = story.PersonName
        }).ToArray();
    }

    public static ProFranchise RefreshMemory(ProFranchise state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var sources = Sources(state);
        return state with
        {
            NarrativeMemory = FootballNarrativeMemoryService.Refresh(
                state.NarrativeMemory,
                sources,
                state.Year,
                FootballRivalryMemoryService.Records(
                    state.Games.Select(game => game.Football).ToArray(),
                    state.Year))
        };
    }

    public static int SocialCount(ProFranchise state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.News.Count + state.NarrativeMemory.Count(record => record.SourceStoryIds.Count >= 2);
    }
}
