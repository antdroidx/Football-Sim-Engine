using DynastyManager.Core.Models;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public sealed class ProNewsIdentityTests
{
    [Fact]
    public async Task SaveLoadMaintainsTheGeneratedNewsSnapshot()
    {
        var root = Path.Combine(Path.GetTempPath(), "pro-news-load-" + Guid.NewGuid());
        try
        {
            var state = ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false);
            state = state with { Transactions = ["2026 W1 · Seattle Seahawks hired Test Coach."] };
            Assert.Empty(state.News);
            var store = new ProSaveStore(root);
            await store.Save(state);
            var loaded = await store.Load(state.Id);

            Assert.Contains(loaded.News, story => story.Category == "Coaching");
            Assert.NotEmpty(loaded.NarrativeMemory);
            Assert.Equal(ProNews.Refresh(loaded).News, loaded.News);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void InjuryStoryCarriesTheRecordedPlayerIdentity()
    {
        var state = ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false);
        var player = state.Players.First(item => item.Football.TeamName == state.UserTeam && !item.Retired);
        var injury = new PlayerInjury
        {
            InjuryId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            SeasonYear = state.Year,
            StartWeek = state.Week,
            BodyArea = InjuryBodyArea.Ankle,
            Severity = InjurySeverity.Moderate,
            InitialWeeks = 2,
            WeeksRemaining = 2
        };
        player = player with { Football = player.Football with { CurrentInjury = injury } };
        state = state with { Players = state.Players.Select(item => item.Football.PlayerId == player.Football.PlayerId ? player : item).ToList() };

        var story = Assert.Single(ProNews.Refresh(state).News, item => item.Category == "Injuries");

        Assert.Equal(FootballPersonKind.Player, story.PersonKind);
        Assert.Equal(player.Football.PlayerId, story.PersonId);
        Assert.Equal(player.Football.FullName, story.PersonName);
    }

    [Fact]
    public void RefreshBackfillsIdentityOntoMatchingLegacyStory()
    {
        var state = ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false);
        var player = state.Players.First(item => item.Football.TeamName == state.UserTeam && !item.Retired);
        var injury = new PlayerInjury
        {
            InjuryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            SeasonYear = state.Year,
            StartWeek = state.Week,
            BodyArea = InjuryBodyArea.Knee,
            Severity = InjurySeverity.Minor,
            InitialWeeks = 1,
            WeeksRemaining = 1
        };
        player = player with { Football = player.Football with { CurrentInjury = injury } };
        state = state with
        {
            Players = state.Players.Select(item => item.Football.PlayerId == player.Football.PlayerId ? player : item).ToList(),
            News = [new ProNewsStory("injury-" + injury.InjuryId, injury.SeasonYear, injury.StartWeek, "Injuries",
                player.Football.FullName + " sidelined", "Legacy story without person metadata.", player.Football.TeamName)]
        };

        var story = Assert.Single(ProNews.Refresh(state).News, item => item.Id == "injury-" + injury.InjuryId);

        Assert.Equal(FootballPersonKind.Player, story.PersonKind);
        Assert.Equal(player.Football.PlayerId, story.PersonId);
        Assert.Equal(player.Football.FullName, story.PersonName);
    }

    [Fact]
    public void ResultStoryCarriesPlayerOfTheGameIdentity()
    {
        var state = ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false);
        var player = state.Players.First(item => item.Football.TeamName == state.UserTeam && !item.Retired);
        var original = state.Games.First(item => item.Phase == ProPhase.RegularSeason);
        var award = new PlayerGameAward
        {
            GameId = original.Football.GameId,
            SeasonYear = state.Year,
            Week = original.Football.Week,
            PlayerId = player.Football.PlayerId,
            PlayerName = player.Football.FullName,
            TeamName = player.Football.TeamName,
            Summary = "Recorded player of the game."
        };
        var game = original with
        {
            Football = original.Football with
            {
                HasPlayed = true,
                HomeScore = 24,
                AwayScore = 17,
                PlayerOfGame = award
            }
        };
        state = state with { Games = [game] };

        var story = Assert.Single(ProNews.Refresh(state).News, item => item.Id == "game-" + game.Football.GameId);

        Assert.Equal(FootballPersonKind.Player, story.PersonKind);
        Assert.Equal(player.Football.PlayerId, story.PersonId);
        Assert.Equal(player.Football.FullName, story.PersonName);
    }

    [Fact]
    public void AwardAndAllStarStoriesCarryPlayerIdentity()
    {
        var state = ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false);
        var player = state.Players.First(item => item.Football.TeamName == state.UserTeam && !item.Retired);
        var award = new ProAward("Test Award", player.Football.PlayerId, player.Football.FullName, player.Football.TeamName, 100);
        var selection = new ProAllStarSelection(player, "NFC");
        state = state with
        {
            Honors = [new ProSeasonHonors(state.Year, [award], [selection])]
        };

        var refreshed = ProNews.Refresh(state).News;
        var awardStory = Assert.Single(refreshed, item => item.Id == $"award-{state.Year}-{award.Title}");
        var selectionStory = Assert.Single(refreshed, item =>
            item.Id == $"allstars-{state.Year}-{selection.Conference}-{player.Football.PlayerId:N}");

        Assert.Equal(player.Football.PlayerId, awardStory.PersonId);
        Assert.Equal(player.Football.FullName, awardStory.PersonName);
        Assert.Equal(player.Football.PlayerId, selectionStory.PersonId);
        Assert.Equal(player.Football.FullName, selectionStory.PersonName);
    }
}
