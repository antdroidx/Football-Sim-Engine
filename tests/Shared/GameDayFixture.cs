using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;

namespace Football.Testing;

public static class GameDayFixture
{
    public static GameDaySession Session(GameDayMode mode, bool includeOpeningKickoff = false, int seed = 7123, bool neutral = false)
    {
        var homePlan = new TeamGamePlan { TeamName = "Home", Offense = OffensivePlaybook.Spread,
            OffensiveFormation = OffensiveFormation.ShotgunSpread, DefensiveFormation = DefensiveFormation.Nickel };
        var awayPlan = homePlan with { TeamName = "Away" };
        var session = GameDayEngine.Start(new ScheduledGame { GameId = "game-day", SeasonYear = 2026, Week = 1,
                HomeTeamName = "Home", AwayTeamName = "Away", SimulationSeed = seed, IsNeutralSite = neutral },
            Profile("Home", homePlan), Profile("Away", awayPlan), Roster(), mode);
        session.PossessionTeamName = "Home"; session.OpeningReceivingTeam = "Home"; session.KickoffTeamName = "Away";
        session.KickoffPending = includeOpeningKickoff;
        return session;
    }

    private static TeamSimulationProfile Profile(string team, TeamGamePlan plan) => new()
    {
        TeamName = team, PassOffenseRating = 78, RushOffenseRating = 76,
        PassDefenseRating = 75, RushDefenseRating = 75, SpecialTeamsRating = 75, GamePlan = plan
    };

    private static DynastyPlayer[] Roster()
    {
        var players = new List<DynastyPlayer>();
        foreach (var team in new[] { "Home", "Away" })
        foreach (var (position, count) in new[] { (Position.QB, 2), (Position.RB, 4), (Position.WR, 7),
                     (Position.TE, 4), (Position.OL, 10), (Position.DE, 5), (Position.DT, 5),
                     (Position.OLB, 5), (Position.MLB, 4), (Position.CB, 7), (Position.FS, 4), (Position.SS, 4) })
        for (var i = 1; i <= count; i++) players.Add(new DynastyPlayer
        {
            PlayerId = Guid.NewGuid(), FullName = $"{team} {position} {i}", TeamName = team, Position = position,
            ClassYear = 2, TalentLevel = 75, OverallRating = 85 - i, PotentialRating = 90,
            SpeedRating = 75, StrengthRating = 75, AgilityRating = 75, AwarenessRating = 75,
            InjuryRating = 80, DepthChartOrder = i
        });
        return players.ToArray();
    }
}
