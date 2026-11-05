using System.Text.Json;
using DynastyManager.Core.Models;
using Xunit;

namespace DynastyManager.Core.Tests;

public class CollegeSocialTests
{
    [Fact]
    public void SocialActivitySurvivesSaveRoundTrip()
    {
        var state = new DynastyState
        {
            DynastyName = "Test Dynasty",
            UserTeamName = "Washington",
            SeasonYear = 2026,
            Week = 4,
            SocialInteractions =
            [
                new FootballSocialInteraction
                {
                    Id = "post-1:Like",
                    PostId = "post-1",
                    Kind = FootballSocialInteractionKind.Like,
                    SeasonYear = 2026,
                    Week = 4
                }
            ],
            SocialFollows =
            [
                new FootballSocialFollow
                {
                    Id = "@dynastywire",
                    Handle = "@DynastyWire",
                    Name = "Dynasty Wire",
                    SeasonYear = 2026
                }
            ]
        };

        var restored = JsonSerializer.Deserialize<DynastyState>(JsonSerializer.Serialize(state))!;

        var interaction = Assert.Single(restored.SocialInteractions);
        Assert.Equal("post-1", interaction.PostId);
        Assert.Equal(FootballSocialInteractionKind.Like, interaction.Kind);
        var follow = Assert.Single(restored.SocialFollows);
        Assert.Equal("@DynastyWire", follow.Handle);
    }

    [Fact]
    public void OlderSaveWithoutSocialActivityLoadsWithEmptyCollections()
    {
        const string legacy = "{\"DynastyName\":\"Legacy\",\"UserTeamName\":\"Washington\",\"SeasonYear\":2026}";
        var restored = JsonSerializer.Deserialize<DynastyState>(legacy)!;

        Assert.Empty(restored.SocialInteractions);
        Assert.Empty(restored.SocialFollows);
    }
}