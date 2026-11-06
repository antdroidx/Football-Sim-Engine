using System.Text.Json;
using DynastyManager.Core.Models;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public class ProSocialTests
{
    [Fact]
    public void SocialActivitySurvivesSaveRoundTrip()
    {
        var team = new ProTeam("SEA", "Seattle", "Seahawks", "SEA", "NFC", "West", 0, "#20608B", "#000000");
        var state = new ProFranchise
        {
            Name = "Test franchise",
            League = new ProLeague(1, "Test League", 53, 16, [team]),
            UserTeam = team.Name,
            Year = 2026,
            Week = 4,
            SocialInteractions =
            [
                new FootballSocialInteraction
                {
                    Id = "post-1:Repost",
                    PostId = "post-1",
                    Kind = FootballSocialInteractionKind.Repost,
                    SeasonYear = 2026,
                    Week = 4
                },
                new FootballSocialInteraction
                {
                    Id = "post-1:Reply",
                    PostId = "post-1",
                    Kind = FootballSocialInteractionKind.Reply,
                    Body = "Pro reply",
                    SeasonYear = 2026,
                    Week = 4
                }
            ],
            SocialFollows =
            [
                new FootballSocialFollow
                {
                    Id = "@league sideline",
                    Handle = "@LeagueSideline",
                    Name = "League Sideline",
                    SeasonYear = 2026
                }
            ]
        };

        var restored = JsonSerializer.Deserialize<ProFranchise>(JsonSerializer.Serialize(state))!;

        Assert.Equal(2, restored.SocialInteractions.Count);
        var interaction = restored.SocialInteractions[0];
        Assert.Equal("post-1", interaction.PostId);
        Assert.Equal(FootballSocialInteractionKind.Repost, interaction.Kind);
        Assert.Equal("Pro reply", restored.SocialInteractions[1].Body);
        var follow = Assert.Single(restored.SocialFollows);
        Assert.Equal("@LeagueSideline", follow.Handle);
    }

    [Fact]
    public void OlderSaveWithoutSocialActivityLoadsWithEmptyCollections()
    {
        var team = new ProTeam("SEA", "Seattle", "Seahawks", "SEA", "NFC", "West", 0, "#20608B", "#000000");
        var legacy = JsonSerializer.Serialize(new ProFranchise
        {
            Name = "Test franchise",
            League = new ProLeague(1, "Test League", 53, 16, [team]),
            UserTeam = team.Name,
            Year = 2026
        }).Replace("\"SocialInteractions\":[],", "").Replace("\"SocialFollows\":[],", "");

        var restored = JsonSerializer.Deserialize<ProFranchise>(legacy)!;

        Assert.Empty(restored.SocialInteractions);
        Assert.Empty(restored.SocialFollows);
    }
}