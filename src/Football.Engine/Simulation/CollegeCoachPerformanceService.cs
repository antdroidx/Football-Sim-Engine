using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Audit-style College coaching performance keyed by recent team results and prestige expectation.</summary>
public static class CollegeCoachPerformanceService
{
    public static CoachPerformanceAudit Audit(
        DynastyState state,
        Team team,
        StaffMember coach)
    {
        var prestige = ProgramPrestigeService.GetCurrentPrestige(state, team);
        var season = LatestSeason(state, team.Name);
        var expectedWins = ExpectedWins(prestige);
        var wins = season?.Wins ?? 0;
        var losses = season?.Losses ?? 0;
        var winDelta = wins - expectedWins;
        var change = PerformanceChange(winDelta);
        var summary = $"Expected {expectedWins}, recorded {wins}-{losses}.";
        return new(
            coach.StaffId,
            coach.FullName,
            team.Name,
            coach.Role,
            season?.SeasonYear ?? state.SeasonYear,
            prestige,
            wins,
            losses,
            expectedWins,
            winDelta,
            change,
            summary);
    }

    public static IReadOnlyList<CoachPerformanceAudit> AuditLeague(
        DynastyState state,
        IReadOnlyList<Team> teams)
    {
        return teams.SelectMany(team =>
        {
            var teamName = team.Name;
            var teamStaff = state.Staff
                .Where(item => item.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase) &&
                    StaffAttributeService.IsFootballCoach(item.Role))
                .ToArray();
            return teamStaff.Select(coach => Audit(state, team, coach));
        }).ToArray();
    }

    public static int ExpectedWins(int prestige) =>
        Math.Clamp(4 + (prestige - 50) / 7, 2, 12);

    public static int PerformanceChange(int winDelta) => winDelta switch
    {
        >= 4 => 2,
        >= 2 => 1,
        <= -4 => -2,
        <= -2 => -1,
        _ => 0
    };

    private static TeamSeasonHistoryRecord? LatestSeason(DynastyState state, string teamName) =>
        state.TeamSeasonHistory
            .Where(item => item.TeamName.Equals(teamName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.SeasonYear)
            .FirstOrDefault();
}
