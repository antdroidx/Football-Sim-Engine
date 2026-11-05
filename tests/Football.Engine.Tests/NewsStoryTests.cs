using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Presentation"),Trait("Category","Fast")]
public class NewsStoryTests
{
    [Fact]
    public void SavedSnapshotRoundTripsAndToggleRemovesOnlySelectedStory()
    {
        var source = new FootballNewsStory
        {
            Id = "result/1", Category = "Results", Headline = "Recorded result", Body = "Recorded facts",
            TeamName = "Home", SeasonYear = 2026, Week = 1
        };
        var state = new DynastyState { DynastyName = "Test", UserTeamName = "Home" };
        Assert.Empty(state.SavedNews);
        state = state with { SavedNews = FootballNewsReadStateService.ToggleSaved(state.SavedNews, source) };
        var restored = System.Text.Json.JsonSerializer.Deserialize<DynastyState>(System.Text.Json.JsonSerializer.Serialize(state))!;
        Assert.Equal(source, Assert.Single(restored.SavedNews));
        var post = Assert.Single(FootballNewsSocialService.Build([source]));
        var both = FootballNewsReadStateService.ToggleSaved(restored.SavedNews, post);
        Assert.Equal(post, Assert.Single(FootballNewsReadStateService.ToggleSaved(both, source)));
    }

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
        Assert.Equal(recorded.Headline, reaction.Headline);
        Assert.StartsWith("Home update ·", reaction.Body);
        Assert.Contains(recorded.Body, reaction.Body);
        Assert.False(string.IsNullOrWhiteSpace(reaction.AuthorName));
        Assert.StartsWith("@", reaction.AuthorHandle!);
        Assert.InRange(reaction.LikeCount, 12, 100);
        Assert.InRange(reaction.RepostCount, 1, 14);
        Assert.InRange(reaction.ReplyCount, 0, 17);
    }


    [Fact]
    public void NarrativeMemoryIsIdempotentAndCreatesFactualSeasonThreadPosts()
    {
        var first = new FootballNewsStory
        {
            Id = "game/1", Category = "Results", Headline = "Home wins opener", Body = "Recorded opener.",
            TeamName = "Home", SeasonYear = 2026, Week = 1
        };
        var second = first with { Id = "game/2", Headline = "Home wins again", Body = "Recorded second result.", Week = 2 };
        var initial = FootballNarrativeMemoryService.Refresh([], [first], 2026);
        var refreshed = FootballNarrativeMemoryService.Refresh(initial, [first, second], 2026);
        var repeated = FootballNarrativeMemoryService.Refresh(refreshed, [first, second], 2026);

        var memory = Assert.Single(repeated);
        Assert.Equal(2, memory.SourceStoryIds.Count);
        Assert.Equal(1, memory.FirstWeek);
        Assert.Equal(2, memory.LastWeek);
        Assert.Equal(second.Headline, memory.LatestHeadline);
        var thread = Assert.Single(FootballNewsSocialService.Build([first, second], repeated), post => post.Id == "social/" + memory.Id);
        Assert.Equal(second.Id, thread.RelatedId);
        Assert.StartsWith("Recorded 2026 updates · Latest:", thread.Body);
        Assert.Contains(second.Headline, thread.Body);
    }

    [Fact]
    public void OlderSavedStoryRecordsDefaultNarrativeMemoryToEmpty()
    {
        var state = new DynastyState { DynastyName = "Test", UserTeamName = "Home" };
        Assert.Empty(state.NarrativeMemory);
        var restored = System.Text.Json.JsonSerializer.Deserialize<DynastyState>("""{"DynastyName":"Legacy","UserTeamName":"Home"}""")!;
        Assert.Empty(restored.NarrativeMemory);
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
