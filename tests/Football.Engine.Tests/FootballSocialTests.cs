using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class FootballSocialTests
{
    [Fact]
    public void LikesAndRepostsAreRecordedAndCleared()
    {
        var interactions = FootballSocialService.Toggle([], "post-1", FootballSocialInteractionKind.Like, 2026, 4);
        var interaction = Assert.Single(interactions);
        Assert.Equal("post-1", interaction.PostId);
        Assert.Equal(FootballSocialInteractionKind.Like, interaction.Kind);
        Assert.Equal(2026, interaction.SeasonYear);

        var reposted = FootballSocialService.Toggle(interactions, "post-1", FootballSocialInteractionKind.Repost, 2026, 4);
        Assert.Equal(2, reposted.Count);

        var cleared = FootballSocialService.Toggle(reposted, "post-1", FootballSocialInteractionKind.Like, 2026, 4);
        Assert.Single(cleared);
        Assert.Equal(FootballSocialInteractionKind.Repost, cleared[0].Kind);
    }

    [Fact]
    public void AppliedPostsShowThePlayersOwnActivity()
    {
        var posts = new[] { Post("post-1", "@DynastyWire"), Post("post-2", "@GridironPulse") };
        var interactions = FootballSocialService.Toggle([], "post-1", FootballSocialInteractionKind.Like, 2026, 4);
        var follows = FootballSocialService.ToggleFollow([], "@GridironPulse", "Gridiron Pulse", 2026);

        var applied = FootballSocialService.Apply(posts, interactions, follows);

        var liked = applied.Single(post => post.Id == "post-1");
        Assert.True(liked.LikedByPlayer);
        Assert.False(liked.RepostedByPlayer);
        Assert.False(liked.FollowedAuthor);

        var followed = applied.Single(post => post.Id == "post-2");
        Assert.True(followed.FollowedAuthor);
    }

    [Fact]
    public void PlayerReplyIsAuthoredPersistedAndShownSeparately()
    {
        var story = new FootballNewsStory
        {
            Id = "story-reply",
            Category = "Results",
            Headline = "Recorded result",
            Body = "Recorded facts.",
            TeamName = "Washington"
        };
        var post = Post("post-reply", "@DynastyWire") with { RelatedId = story.Id };
        var interactions = FootballSocialService.Reply([], post.Id, "My saved reply.", 2026, 4);

        var interaction = Assert.Single(interactions);
        Assert.Equal(FootballSocialInteractionKind.Reply, interaction.Kind);
        Assert.Equal("My saved reply.", interaction.Body);
        Assert.True(Assert.Single(FootballSocialService.Apply([post], interactions, [])).RepliedByPlayer);

        var replies = FootballSocialService.BuildReplies([post], [story], interactions, 2026, 4);
        Assert.Equal(2, replies.Count);
        var playerReply = Assert.Single(replies, reply => reply.AuthoredByPlayer);
        Assert.Equal(post.Id, playerReply.RelatedId);
        Assert.Equal("My saved reply.", playerReply.Body);
        Assert.Single(replies, reply => !reply.AuthoredByPlayer);

        var replaced = FootballSocialService.Reply(interactions, post.Id, "Updated reply.", 2026, 4);
        Assert.Equal("Updated reply.", Assert.Single(replaced).Body);
        Assert.Empty(FootballSocialService.Reply(replaced, post.Id, "  ", 2026, 4));
        Assert.Throws<ArgumentException>(() => FootballSocialService.Toggle([], post.Id, FootballSocialInteractionKind.Reply, 2026, 4));
    }

    [Fact]
    public void FollowedDesksComeFirstInTheFeed()
    {
        var posts = new[]
        {
            Post("post-1", "@DynastyWire") with { SeasonYear = 2026, Week = 4 },
            Post("post-2", "@GridironPulse") with { SeasonYear = 2026, Week = 4 }
        };
        var follows = FootballSocialService.ToggleFollow([], "@GridironPulse", "Gridiron Pulse", 2026);

        var applied = FootballSocialService.Apply(posts, [], follows);

        Assert.Equal("post-2", applied[0].Id);
    }

    [Fact]
    public void RepliesRestateTheRecordedCategoryAndComeFromAnotherDesk()
    {
        var story = new FootballNewsStory
        {
            Id = "story-1",
            Category = "Injuries",
            Headline = "Injury report",
            Body = "A player is out.",
            TeamName = "Washington"
        };
        var post = new FootballNewsStory
        {
            Id = "social/story-1",
            Category = "In-game social",
            Headline = "Washington update",
            Body = "Washington update · A player is out.",
            TeamName = "Washington",
            RelatedId = "story-1",
            AuthorName = "Dynasty Wire",
            AuthorHandle = "@DynastyWire",
            MediaKind = FootballNewsMediaKind.Social
        };

        var replies = FootballSocialService.ComposeReplies([post], [story], 2026, 4);

        var reply = Assert.Single(replies);
        Assert.Equal(post.Id, reply.RelatedId);
        Assert.Equal("Injury report noted.", reply.Body);
        Assert.NotEqual(post.AuthorHandle, reply.AuthorHandle);
        Assert.Equal(FootballNewsMediaKind.SocialReply, reply.MediaKind);
    }

    [Fact]
    public void RivalryThreadsComeFromTheRecordedResults()
    {
        var games = new[]
        {
            Game(2024, 1), Game(2024, 7), Game(2025, 3)
        };

        var records = FootballRivalryMemoryService.Records(games, 2025);

        var record = Assert.Single(records);
        Assert.Equal(FootballNarrativeKind.Rivalry, record.Kind);
        Assert.Equal("Washington", record.TeamName);
        Assert.Equal("Rivalry vs Oregon", record.Topic);
        Assert.Contains("met 3 times", record.LatestHeadline);
    }

    [Fact]
    public void RivalryThreadsStayEmptyWithoutARecordedSeries()
    {
        var records = FootballRivalryMemoryService.Records([Game(2025, 1)], 2025);

        Assert.Empty(records);
    }

    static FootballNewsStory Post(string id, string handle) => new()
    {
        Id = id,
        Category = "In-game social",
        Headline = "Post",
        Body = "Body",
        TeamName = "Washington",
        AuthorName = "Dynasty Wire",
        AuthorHandle = handle,
        MediaKind = FootballNewsMediaKind.Social
    };

    static ScheduledGame Game(int seasonYear, int week) => new()
    {
        GameId = $"{seasonYear}-{week}",
        SeasonYear = seasonYear,
        Week = week,
        HomeTeamName = "Washington",
        AwayTeamName = "Oregon"
    };
}