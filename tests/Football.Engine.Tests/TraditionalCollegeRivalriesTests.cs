using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using System.Text.Json;
using Xunit;

namespace Football.Engine.Tests;

[Trait("Owner","Engine"),Trait("Category","Fast")]
public class TraditionalCollegeRivalriesTests
{
    [Fact]
    public void TraditionalPairingsAreUniqueDoNotSelfPairAndAreLimitedToThreePerTeam()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var (teamA, teamB, _) in TraditionalCollegeRivalries.Pairs)
        {
            Assert.NotEqual(teamA, teamB, StringComparer.OrdinalIgnoreCase);

            var key = StringComparer.OrdinalIgnoreCase.Compare(teamA, teamB) <= 0
                ? $"{teamA}|{teamB}"
                : $"{teamB}|{teamA}";

            Assert.True(seen.Add(key), $"Duplicate rivalry pair: {key}");
            counts[teamA] = counts.GetValueOrDefault(teamA) + 1;
            counts[teamB] = counts.GetValueOrDefault(teamB) + 1;
        }

        Assert.All(counts, entry => Assert.InRange(entry.Value, 1, 3));
    }

    [Fact]
    public void TraditionalPairingsContainAtLeastOneAnnualRivalry()
    {
        Assert.Contains(TraditionalCollegeRivalries.Pairs, pair => pair.Annual);
    }

    [Fact]
    public void TraditionalDetectionIsCaseInsensitiveForNamedPairings()
    {
        var teams = new[]
        {
            new Team { Name = "Notre Dame", Abbreviation = "ND", ConferenceName = "Independent" },
            new Team { Name = "USC", Abbreviation = "USC", ConferenceName = "Big Ten" }
        };

        Assert.True(TraditionalCollegeRivalries.IsTraditional(teams, "USC", "Notre Dame"));
        Assert.False(TraditionalCollegeRivalries.IsTraditional(teams, "USC", "Washington"));
    }
}

