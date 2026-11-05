using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class CoachHallOfFameTests
{
    static CoachHallOfFameCandidate Candidate(double winPercentage = 0.7,
        int conferenceTitles = 1, int playoffAppearances = 2, int nationalTitles = 1,
        bool careerComplete = true, Guid? staffId = null)
    {
        var id = staffId ?? Guid.NewGuid();
        var seasons = new List<CoachSeasonRecord>();
        for (var i = 0; i < 8; i++)
        {
            seasons.Add(new CoachSeasonRecord
            {
                StaffId = id,
                SeasonYear = 2018 + i,
                CoachName = "Test Coach",
                TeamName = "College",
                Wins = (int)Math.Round(winPercentage * 12),
                Losses = 12 - (int)Math.Round(winPercentage * 12),
                Role = StaffRole.HeadCoach,
                ConferenceChampion = i < conferenceTitles,
                PlayoffParticipant = i < playoffAppearances,
                NationalChampion = i < nationalTitles
            });
        }
        return new CoachHallOfFameCandidate
        {
            CoachId = id,
            CoachName = "Test Coach",
            Seasons = seasons,
            CareerComplete = careerComplete,
            RetiredYear = careerComplete ? 2026 : null
        };
    }

    [Fact]
    public void CoachHallOfFameRequiresQualifyingRecordAndHonors()
    {
        var entries = CoachHallOfFameService.Build([Candidate()], 2026);
        var entry = Assert.Single(entries);
        Assert.Equal(2026, entry.InductionYear);
        Assert.Equal("Test Coach", entry.CoachName);
        Assert.Equal(1, entry.ConferenceTitles);
        Assert.Equal(2, entry.PlayoffAppearances);
        Assert.Equal(1, entry.NationalTitles);
        Assert.Contains("2 playoff appearances", entry.Citation);
        Assert.True(entry.Retired);
        Assert.Equal(2026, entry.RetiredYear);
    }

    [Fact]
    public void CoachHallOfFameExcludesMissingTitleSuccess()
    {
        var entries = CoachHallOfFameService.Build([Candidate(conferenceTitles: 0, playoffAppearances: 0, nationalTitles: 0)], 2026);
        Assert.Empty(entries);
    }

    [Fact]
    public void CoachHallOfFameRequiresACompletedCareer()
    {
        var entries = CoachHallOfFameService.Build([Candidate(careerComplete: false)], 2026);

        Assert.Empty(entries);
    }

    [Fact]
    public void CoachHallOfFameRequiresTheMinimumSeasonsAndWinPercentage()
    {
        var shortTenure = Candidate() with
        {
            Seasons = Candidate().Seasons.Take(3).ToArray()
        };
        Assert.Empty(CoachHallOfFameService.Build([shortTenure], 2026));

        var poorRecord = Candidate(winPercentage: .40);
        Assert.Empty(CoachHallOfFameService.Build([poorRecord], 2026));
    }

    [Fact]
    public void TeamCitationsListEveryStopWithItsRecord()
    {
        var id = Guid.NewGuid();
        var seasons = new List<CoachSeasonRecord>();
        for (var i = 0; i < 4; i++)
            seasons.Add(Season(id, 2018 + i, "Washington", 9, 3, i < 2));
        for (var i = 0; i < 3; i++)
            seasons.Add(Season(id, 2022 + i, "Oregon", 7, 5, false));

        var entries = CoachHallOfFameService.Build(
        [
            new CoachHallOfFameCandidate
            {
                CoachId = id,
                CoachName = "Test Coach",
                Seasons = seasons,
                CareerComplete = true
            }
        ], 2026);

        var entry = Assert.Single(entries);
        Assert.Equal(2, entry.TeamCitations.Length);
        Assert.Contains("Washington 2018-2021", entry.TeamCitations[0]);
        Assert.Contains("Oregon 2022-2024", entry.TeamCitations[1]);
        Assert.Equal("Oregon", entry.TeamName);
    }

    static CoachSeasonRecord Season(Guid id, int year, string team, int wins, int losses, bool playoff) => new()
    {
        StaffId = id,
        CoachName = "Test Coach",
        SeasonYear = year,
        TeamName = team,
        Wins = wins,
        Losses = losses,
        Role = StaffRole.HeadCoach,
        PlayoffParticipant = playoff
    };
}