using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class HallOfFameService
{
    // Original Java Team.checkHallofFame thresholds and award weights. Use
    // recorded career production only; never invent stats for imported careers.
    public static DynastyState Induct(DynastyState state, IEnumerable<PlayerDepartureRecord> departures)
    {
        var entries = state.HallOfFame.ToList();
        var statistics = state.PlayerSeasonStats.Concat(DynastyHistoryService.BuildCurrentPlayerStats(state))
            .GroupBy(p => (p.PlayerId, p.SeasonYear, p.TeamName)).Select(g => g.Last()).ToArray();
        foreach (var departure in departures.Where(d => d.Reason is PlayerDepartureReason.Graduation or PlayerDepartureReason.EarlyProDeclaration))
        {
            if (entries.Any(e => e.PlayerId == departure.PlayerId)) continue;
            var lines = statistics.Where(p => p.PlayerId == departure.PlayerId).ToArray();
            var honors = state.PlayerAwardHistory.Where(a => a.PlayerId == departure.PlayerId && !a.IsPreseason)
                .DistinctBy(a => (a.SeasonYear, a.AwardName, a.ConferenceName, a.SelectionTeam)).ToArray();
            var awardScore = honors.Sum(a => a.ConferenceName is not null ? 10 : a.IsAllAmerican ? 25 :
                a.AwardName.Contains("Heisman", StringComparison.OrdinalIgnoreCase) ? 60 : 30);
            var years = Math.Max(1, lines.Select(l => l.SeasonYear).Distinct().Count());
            var score = (int)lines.Sum(l => Production(l)) + departure.OverallRating * 10 * years;
            var total = score + awardScore * 10;
            if (lines.Length == 0 && honors.Length == 0) continue;
            if (awardScore <= 99 && score <= 28000 && total <= 28750 && (years < 2 || total / years <= 7450)) continue;
            entries.Add(new HallOfFameEntry { PlayerId = departure.PlayerId, PlayerName = departure.FullName,
                TeamName = departure.TeamName, Position = departure.Position, InductionYear = departure.SeasonYear,
                CareerScore = score, HonorsScore = awardScore, RecordedSeasons = years,
                Citation = $"{years} recorded seasons · {honors.Length} final honors · career score {score:N0} · honors {awardScore}" });
        }
        return state with { HallOfFame = entries };
    }
    private static double Production(PlayerSeasonStatLine p) => PlayerCareerScoreService.Production(p);
}
