using System.Text.Json;
using DynastyManager.Core.Models;
using ProFootball.Core;
using Xunit;

namespace ProFootball.Tests;

public class ProCoachHallOfFameTests
{
    [Fact]
    public void ACompletedProCoachingCareerIsInducted()
    {
        var next = ProCoachHallOfFameService.Refresh(State(unemployed: true));

        var entry = Assert.Single(next.CoachHallOfFame);
        Assert.Equal("Test Coach", entry.CoachName);
        Assert.Equal(2026, entry.InductionYear);
        Assert.Equal(8, entry.HeadCoachSeasons);
        Assert.Equal(1, entry.NationalTitles);
        Assert.NotEmpty(entry.TeamCitations);
    }

    [Fact]
    public void CpuCoachSeasonHistoryIsInductedAfterTheCareerEnds()
    {
        var next = ProCoachHallOfFameService.Refresh(CpuState(current: false));

        var entry = Assert.Single(next.CoachHallOfFame);
        Assert.Equal("CPU Coach", entry.CoachName);
        Assert.Equal(6, entry.HeadCoachSeasons);
        Assert.Equal(1, entry.NationalTitles);
    }

    [Fact]
    public void EmployedCpuCoachIsNotInducted()
    {
        Assert.Empty(ProCoachHallOfFameService.Refresh(CpuState(current: true)).CoachHallOfFame);
    }

    [Fact]
    public void SameNameClubCoachesKeepDistinctStableIdentities()
    {
        var state = ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false);
        var firstTeam = state.League.Teams[0];
        var secondTeam = state.League.Teams[1];
        var first = ProStaff.InitializeMetadata(
            new ProCoach(firstTeam.Id, StaffRole.HeadCoach, "Same Name", "Head Coach", "Generated"), state.Id);
        var second = ProStaff.InitializeMetadata(
            new ProCoach(secondTeam.Id, StaffRole.OffensiveCoordinator, "Same Name", "Offensive Coordinator", "Generated"), state.Id);

        Assert.NotNull(first.StaffId);
        Assert.NotNull(second.StaffId);
        Assert.NotEqual(first.StaffId, second.StaffId);
        Assert.Equal(first.StaffId, ProStaff.InitializeMetadata(first with { Age = 0 }, state.Id).StaffId);
    }

    [Fact]
    public void CoachConferenceRecordIncludesCrossDivisionGamesButSeparatesTies()
    {
        var state = ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false);
        var team = state.League.Teams.Single(item => item.Name == state.UserTeam);
        var division = state.League.Teams.First(item =>
            item.Conference == team.Conference && item.Division == team.Division && item.Id != team.Id);
        var crossDivision = state.League.Teams.First(item =>
            item.Conference == team.Conference && item.Division != team.Division);
        var nonConference = state.League.Teams.First(item => item.Conference != team.Conference);
        var games = new[]
        {
            Result(state, team, crossDivision, 21, 14),
            Result(state, team, division, 10, 17),
            Result(state, team, nonConference, 30, 10),
            Result(state, team, crossDivision, 20, 20)
        };
        var coach = ProStaff.InitializeMetadata(
            state.Coaches.Single(item => item.TeamId == team.Id && item.Role == StaffRole.HeadCoach), state.Id);
        state = state with
        {
            Coaches = [coach],
            Games = games.ToList(),
            CoachHistoryLastRecordedSeasonYear = 0
        };

        var record = Assert.Single(ProCoachHistoryService.RecordCompletedSeason(state).CoachSeasonHistory);

        Assert.Equal(2, record.Wins);
        Assert.Equal(1, record.Losses);
        Assert.Equal(1, record.Ties);
        Assert.Equal(1, record.ConferenceWins);
        Assert.Equal(1, record.ConferenceLosses);
    }

    [Fact]
    public void CompletedCpuCoachSeasonsAreRecordedOnce()
    {
        var state = ClubSeasonState();
        var first = ProCoachHistoryService.RecordCompletedSeason(state);
        var second = ProCoachHistoryService.RecordCompletedSeason(first);

        Assert.Equal(4, first.CoachSeasonHistory.Count);
        Assert.All(first.CoachSeasonHistory, record => Assert.Equal(2026, record.SeasonYear));
        Assert.All(first.CoachSeasonHistory, record => Assert.Equal(1, record.Wins + record.Losses + record.Ties));
        Assert.Equal(first.CoachSeasonHistory, second.CoachSeasonHistory);
    }

    [Fact]
    public void ACoachStillWorkingIsNotInducted()
    {
        var next = ProCoachHallOfFameService.Refresh(State(unemployed: false));

        Assert.Empty(next.CoachHallOfFame);
    }

    [Fact]
    public void InductionIsRecordedOnceAndKeepsItsYear()
    {
        var first = ProCoachHallOfFameService.Refresh(State(unemployed: true));
        var second = ProCoachHallOfFameService.Refresh(first with { Year = 2027 });

        var entry = Assert.Single(second.CoachHallOfFame);
        Assert.Equal(2026, entry.InductionYear);
    }

    [Fact]
    public void OlderSaveWithoutCoachInductionsLoadsWithAnEmptyCollection()
    {
        var legacy = JsonSerializer.Serialize(State(unemployed: true))
            .Replace("\"CoachHallOfFame\":[],", "");
        var restored = JsonSerializer.Deserialize<ProFranchise>(legacy)!;

        Assert.Empty(restored.CoachHallOfFame);
        Assert.Empty(restored.CoachSeasonHistory);
    }

    [Fact]
    public void CoachInductionsSurviveSaveRoundTrip()
    {
        var state = ProCoachHallOfFameService.Refresh(State(unemployed: true));
        var restored = JsonSerializer.Deserialize<ProFranchise>(JsonSerializer.Serialize(state))!;

        Assert.Equal(state.CoachHallOfFame.Count, restored.CoachHallOfFame.Count);
        Assert.Equal(state.CoachHallOfFame[0].InductionYear, restored.CoachHallOfFame[0].InductionYear);
        Assert.Equal(state.CoachSeasonHistory.Count, restored.CoachSeasonHistory.Count);
    }

    static ProFranchise CpuState(bool current)
    {
        const string coachName = "CPU Coach";
        var franchiseId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var team = new ProTeam("SEA", "Seattle", "Seahawks", "SEA", "NFC", "West", 0, "#20608B", "#000000");
        var history = Enumerable.Range(2018, 6).Select(year => new CoachSeasonRecord
        {
            StaffId = ProCoachHistoryService.CoachId(franchiseId, team.Id, StaffRole.HeadCoach, coachName),
            CoachName = coachName,
            SeasonYear = year,
            TeamName = team.Name,
            Role = StaffRole.HeadCoach,
            Wins = 11,
            Losses = 5,
            NationalChampion = year == 2020
        }).ToList();
        return new ProFranchise
        {
            Id = franchiseId,
            Name = "CPU history",
            League = new ProLeague(1, "Test League", 53, 16, [team]),
            UserTeam = team.Name,
            Year = 2026,
            CoachSeasonHistory = history,
            Coaches = current
                ? [new ProCoach(team.Id, StaffRole.HeadCoach, coachName, "Head Coach", "Generated")
                {
                    StaffId = ProCoachHistoryService.CoachId(franchiseId, team.Id, StaffRole.HeadCoach, coachName)
                }]
                : []
        };
    }

    static ProGame Result(ProFranchise state, ProTeam home, ProTeam away, int homeScore, int awayScore) => new(
        new ScheduledGame
        {
            GameId = $"{state.Year}-{home.Id}-{away.Id}-{homeScore}-{awayScore}",
            SeasonYear = state.Year,
            Week = 1,
            HomeTeamName = home.Name,
            AwayTeamName = away.Name,
            HomeScore = homeScore,
            AwayScore = awayScore,
            HasPlayed = true
        }, ProPhase.RegularSeason);

    static ProFranchise ClubSeasonState()
    {
        var state = ProLeagueService.Create("Seattle Seahawks", generatedPlayers: true, applyOpeningTrades: false);
        var club = state.League.Teams.Single(team => team.Name == state.UserTeam);
        var game = state.Games.First(item => item.Phase == ProPhase.RegularSeason && item.Football.InvolvesTeam(club.Name));
        return state with
        {
            Coaches = state.Coaches.Where(coach => coach.TeamId == club.Id).ToList(),
            Games = [game with { Football = game.Football with { HasPlayed = true, HomeScore = 21, AwayScore = 14 } }]
        };
    }

    static ProFranchise State(bool unemployed)
    {
        var team = new ProTeam("SEA", "Seattle", "Seahawks", "SEA", "NFC", "West", 0, "#20608B", "#000000");
        var league = new ProLeague(1, "Test League", 53, 16, [team]);
        var coaching = new List<ProCareerSeason>();
        for (var i = 0; i < 8; i++)
            coaching.Add(new ProCareerSeason(2018 + i, team.Name, StaffRole.HeadCoach, 11, 5, 80, 90, false));

        var archives = new List<SeasonArchive>();
        for (var i = 0; i < 4; i++)
        {
            var year = 2018 + i;
            archives.Add(new SeasonArchive(year, year == 2020 ? team.Name : "Other Team",
            [
                new ProGame(new ScheduledGame
                {
                    GameId = $"playoff-{year}",
                    SeasonYear = year,
                    Week = 1,
                    HomeTeamName = team.Name,
                    AwayTeamName = "Other Team"
                }, ProPhase.Playoffs)
            ]));
        }

        return new ProFranchise
        {
            Name = "Test franchise",
            League = league,
            UserTeam = team.Name,
            Year = 2026,
            Week = 1,
            Career = new ProUserCareer("Test Coach", StaffRole.HeadCoach, false) { Unemployed = unemployed },
            CoachingHistory = coaching,
            History = archives
        };
    }
}