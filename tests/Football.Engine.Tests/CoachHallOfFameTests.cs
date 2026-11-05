using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class CoachHallOfFameTests
{
    static CoachCareerSummary Summary(double winPercentage = 0.7,
        int conferenceTitles = 1, int playoffAppearances = 2, int nationalTitles = 1)
    {
        var seasons = new List<CoachSeasonRecord>();
        for (var i = 0; i < 8; i++)
        {
            seasons.Add(new CoachSeasonRecord
            {
                StaffId = Guid.NewGuid(),
                SeasonYear = 2018 + i,
                CoachName = "Test Coach",
                TeamName = "College",
                Wins = 8,
                Losses = 4,
                Role = StaffRole.HeadCoach,
                ConferenceChampion = i < conferenceTitles,
                PlayoffParticipant = i < playoffAppearances,
                NationalChampion = i < nationalTitles
            });
        }
        seasons[0] = seasons[0] with { Wins = 8, Losses = 2 };
        return new CoachCareerSummary { StaffId = Guid.NewGuid(), CoachName = "Test Coach", Seasons = seasons };
    }

    [Fact]
    public void CoachHallOfFameRequiresQualifyingRecordAndHonors()
    {
        var entries = CoachHallOfFameService.Build([Summary()], 2026);
        var entry = Assert.Single(entries);
        Assert.Equal(2026, entry.InductionYear);
        Assert.Equal("Test Coach", entry.CoachName);
        Assert.Equal(1, entry.ConferenceTitles);
        Assert.Equal(2, entry.PlayoffAppearances);
        Assert.Equal(1, entry.NationalTitles);
        Assert.Contains("2 playoff appearances", entry.Citation);
    }

    [Fact]
    public void CoachHallOfFameExcludesMissingTitleSuccess()
    {
        var entries = CoachHallOfFameService.Build([Summary(conferenceTitles:0,playoffAppearances:0,nationalTitles:0)], 2026);
        Assert.Empty(entries);
    }
}
