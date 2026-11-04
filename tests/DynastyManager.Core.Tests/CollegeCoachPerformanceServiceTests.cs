using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace DynastyManager.Core.Tests;

public class CollegeCoachPerformanceServiceTests
{
    [Fact]
    public void AuditUsesSeasonRecordAgainstPrestigeExpectation()
    {
        var coach = new StaffMember { FullName = "Coach", TeamName = "Team", Role = StaffRole.HeadCoach };
        var team = ProgramPrestige(80);
        var season = new TeamSeasonHistoryRecord
        {
            SeasonYear = 2026,
            TeamName = "Team",
            Wins = 10,
            Losses = 3
        };

        var state = new DynastyState
        {
            DynastyName = "Audit",
            UserTeamName = "Team",
            SeasonYear = 2026,
            LeagueTeams = [team],
            ProgramPrestigeHistory =
            [
                new ProgramPrestigeSnapshot
                {
                    SeasonYear = 2026,
                    TeamName = "Team",
                    StartingPrestige = 80,
                    EndingPrestige = 80
                }
            ],
            TeamSeasonHistory = [season]
        };

        var audit = CollegeCoachPerformanceService.Audit(state, team, coach);
        Assert.Equal(CollegeCoachPerformanceService.ExpectedWins(80), audit.ExpectedWins);
        Assert.Equal(2, audit.WinDelta);
        Assert.Equal(1, audit.PerformanceChange);
        Assert.Contains("10-3", audit.Summary);
    }

    [Fact]
    public void BetterStaffCreationScalesAboveThePreviousCeilingOnHighPrestigePrograms()
    {
        var teams = new[]
        {
            Elite(),
            Low()
        };
        var high = StaffManagementService.EnsureLeagueStaff(
            new DynastyState
            {
                DynastyName = "Ratings baseline",
                UserTeamName = "Low",
                SeasonYear = 2026,
                LeagueTeams = teams
            },
            teams);

        var eliteMean = CoachMean(high, "Elite");
        var lowMean = CoachMean(high, "Low");

        Assert.True(eliteMean > lowMean);
        Assert.InRange(
            high.Staff
                .Where(item => StaffAttributeService.IsFootballCoach(item.Role))
                .SelectMany(item => item.Coaching!.Values())
                .Average(value => value.Rating),
            60,
            86);
    }

    static int CoachMean(DynastyState state, string teamName) =>
        (int)Math.Round(
            StaffManagementService.GetTeamStaff(state, teamName)
                .Where(item => StaffAttributeService.IsFootballCoach(item.Role))
                .SelectMany(item => item.Coaching!.Values())
                .Average(value => value.Rating));

    static Team ProgramPrestige(int prestige) => new()
    {
        Name = "Team",
        Abbreviation = "T",
        ConferenceName = "Test",
        Prestige = prestige,
        LegacyRegionId = 1
    };

    static Team Elite() => ProgramPrestige(96) with { Name = "Elite", Abbreviation = "E" };
    static Team Low() => ProgramPrestige(35) with { Name = "Low", Abbreviation = "L" };
}


