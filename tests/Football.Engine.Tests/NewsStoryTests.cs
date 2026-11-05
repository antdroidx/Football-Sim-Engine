using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Presentation"),Trait("Category","Fast")]
public class NewsStoryTests
{
    [Fact]
    public void ReadingSocialPostPreservesUnreadSourceAndDoesNotGenerateReactionsToReactions()
    {
        var source = new FootballNewsStory
        {
            Id = "result/1", Category = "Results", Headline = "Recorded result", Body = "Recorded facts",
            TeamName = "Home", SeasonYear = 2026, Week = 1
        };
        var post = Assert.Single(FootballNewsSocialService.Build([source]));
        var read = FootballNewsReadStateService.MarkRead([], [post.Id]);
        Assert.Equal(source.Id, Assert.Single(FootballNewsReadStateService.Unread([source, post], read)).Id);
        Assert.Empty(FootballNewsSocialService.Build([post]));
        Assert.Equal(post, Assert.Single(FootballNewsSocialService.Build([source])));
    }

    [Fact]
    public void StoryDefaultsToRecordedNarrativeAndReadStateIsFiltered()
    {
        var story = new FootballNewsStory
        {
            Id = "game/123", Category = "Results", Headline = "Recorded result",
            Body = "Recorded play.", TeamName = "Home", SeasonYear = 2026, Week = 1,
            RelatedId = "game-1"
        };
        Assert.Equal(FootballNewsMediaKind.Story, story.MediaKind);
        Assert.False(story.IsSocial);

        var read = FootballNewsReadStateService.MarkRead([], [story.Id, story.Id, ""]);
        var unread = FootballNewsReadStateService.Unread([story, story], read);
        Assert.Equal([story.Id], read);
        Assert.Empty(unread);
    }

    [Fact]
    public void SocialStoryIsExplicitlyLabeled() =>
        Assert.True(new FootballNewsStory
        {
            Id = "social/1", Category = "Reaction", Headline = "In-game media", Body = "Recorded source.",
            TeamName = "Home", SeasonYear = 2026, Week = 1,
            MediaKind = FootballNewsMediaKind.Social
        }.IsSocial);

    [Fact]
    public void SocialPostsDeriveFromRecordedStoriesWithoutInventingStats()
    {
        var recorded = new FootballNewsStory
        {
            Id = "game/123", Category = "Results", Headline = "Recorded result", Body = "Recorded play.",
            TeamName = "Home", SeasonYear = 2026, Week = 1
        };
        var social = FootballNewsSocialService.Build([recorded]);

        var reaction = Assert.Single(social);
        Assert.Equal(FootballNewsMediaKind.Social, reaction.MediaKind);
        Assert.Equal("social/game/123", reaction.Id);
        Assert.StartsWith("In-game media reaction to Home", reaction.Body);
        Assert.Contains(recorded.Headline, reaction.Body);
    }

    [Fact]
    public void HubMergesRecordedAndSocialStoriesWithoutDuplicates()
    {
        var recorded = new FootballNewsStory
        {
            Id = "game/123", Category = "Results", Headline = "Recorded result", Body = "Recorded play.",
            TeamName = "Home", SeasonYear = 2026, Week = 1
        };
        var stories = FootballNewsHubService.Build([recorded]);
        Assert.Equal(2, stories.Count);
        Assert.Equal(recorded.Id, stories[0].Id);
        Assert.Equal(FootballNewsMediaKind.Social, stories[1].MediaKind);
    }
}
