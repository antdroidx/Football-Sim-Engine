using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>
/// Season-zero College identities. The source table is Playbook Gamer's College Football 27 playbook reference,
/// retrieved 2026-11-01. Gameplay uses the shared playbook enums; the source offense and defense labels are retained
/// so future playbooks can map them more precisely as the engine adds concepts.
/// </summary>
public sealed record CollegeTeamStrategy(
    string TeamName,
    string SourceOffense,
    string SourceDefense,
    OffensivePlaybook Offense,
    DefensivePlaybook Defense,
    DefensiveApproach Approach,
    OffensiveFormation OffensiveFormation,
    DefensiveFormation DefensiveFormation,
    OffensiveTempo Tempo)
{
    public TeamGamePlan GamePlan => new()
    {
        TeamName = TeamName,
        Offense = Offense,
        Defense = Defense,
        Approach = Approach,
        OffensiveFormation = OffensiveFormation,
        DefensiveFormation = DefensiveFormation,
        Tempo = Tempo
    };
}

public static class CollegeTeamStrategyCatalog
{
    public const string SourceUrl = "https://playbookgamer.com/college-football-27-playbooks/";
    static readonly Lazy<IReadOnlyDictionary<string, CollegeTeamStrategy>> StrategyIndex =
        new(() => Entries!.ToDictionary(item => item.TeamName, StringComparer.OrdinalIgnoreCase));
    static IReadOnlyDictionary<string, CollegeTeamStrategy> Strategies => StrategyIndex.Value;
    public static IReadOnlyList<CollegeTeamStrategy> All => Entries;
    public static bool TryGet(string team, out CollegeTeamStrategy strategy) =>
        Strategies.TryGetValue(team, out strategy!);
    public static CollegeTeamStrategy? ForTeam(string team) => Strategies.GetValueOrDefault(team);
    public static TeamGamePlan? PlanFor(string team) => ForTeam(team)?.GamePlan;

    public static DynastyState InitializePlans(DynastyState state, IEnumerable<Team> teams)
    {
        var plans = state.TeamGamePlans.ToList();
        var existing = plans.Select(plan => plan.TeamName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var team in teams)
        {
            if (existing.Contains(team.Name) || PlanFor(team.Name) is not { } plan) continue;
            plans.Add(plan);
            existing.Add(team.Name);
        }
        return plans.Count == state.TeamGamePlans.Count ? state : state with { TeamGamePlans = plans.ToArray() };
    }

    static readonly CollegeTeamStrategy[] Entries =
    [
        new("Air Force", "Option", "3-4 Zone", OffensivePlaybook.Option, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Akron", "Spread", "3-4 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Alabama", "Multiple", "3-4 Zone", OffensivePlaybook.Balanced, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Appalachian State", "Spread", "4-2-5 Man", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Arizona", "Multiple", "3-3-5", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Arizona State", "Multiple", "4-2-5 Zone Pressure", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Arkansas", "Spread", "3-3-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Arkansas State", "Power Spread", "4-2-5", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Army", "Option", "3-4 Zone Pressure", OffensivePlaybook.Option, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.Pistol, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Auburn", "Veer & Shoot", "3-3-5 Tite", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Ball State", "Spread", "Multiple", OffensivePlaybook.Spread, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Baylor", "Air Raid", "3-3-5 Shell", OffensivePlaybook.AirRaid, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("Boise State", "Multiple", "4-2-5 Man", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Boston College", "Multiple", "4-3", OffensivePlaybook.Balanced, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("Bowling Green", "Spread", "4-2-5 Man", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Buffalo", "Spread", "4-3", OffensivePlaybook.Spread, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("BYU", "Power Spread", "4-3 Man Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.FourThree, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.FourThree, OffensiveTempo.ChewClock),
        new("California", "Multiple", "3-4 Multiple", OffensivePlaybook.Balanced, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Central Michigan", "Spread Option", "3-4 Zone", OffensivePlaybook.Option, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Charlotte", "Spread", "4-2-5 Zone", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Cincinnati", "Pistol", "3-4 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Clemson", "Power Spread", "Multiple", OffensivePlaybook.PowerRun, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Coastal Carolina", "Spread", "4-2-5 Shell", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Colorado", "Power Spread", "4-2-5 Man Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Colorado State", "Spread", "3-3-5 Tite", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Delaware", "Spread", "3-3-5 Three High", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Duke", "Veer & Shoot", "4-2-5 Man", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("East Carolina", "Air Raid", "3-3-5 Three High", OffensivePlaybook.AirRaid, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("Eastern Michigan", "Spread", "4-3 Man", OffensivePlaybook.Spread, DefensivePlaybook.FourThree, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("Florida", "Power Spread", "3-4 Multiple", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Florida Atl", "Air Raid", "3-3-5 Tite", OffensivePlaybook.AirRaid, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("Florida Intl", "Power Spread", "Multiple", OffensivePlaybook.PowerRun, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Florida State", "Spread", "3-3-5 Zone", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Fresno State", "Spread", "4-2-5 Zone", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Georgia", "Multiple", "3-3-5 Tite", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Georgia Southern", "Spread", "3-4", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Georgia State", "Spread", "3-3-5", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Georgia Tech", "Power Spread", "3-3-5 Zone Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Hawaii", "Run & Shoot", "3-4 Man Pressure", OffensivePlaybook.AirRaid, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.Empty, DefensiveFormation.ThreeFour, OffensiveTempo.UpTempo),
        new("Houston", "Power Spread", "3-3-5 Zone Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Illinois", "Multiple", "3-4 Man", OffensivePlaybook.Balanced, DefensivePlaybook.ThreeFour, DefensiveApproach.ProtectPass, OffensiveFormation.Singleback, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Indiana", "Power Spread", "4-3 Zone Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.FourThree, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.FourThree, OffensiveTempo.ChewClock),
        new("Iowa", "Pro Style", "4-3 Shell", OffensivePlaybook.Balanced, DefensivePlaybook.FourThree, DefensiveApproach.ProtectPass, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("Iowa State", "Multiple", "4-2-5 Zone", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Jacksonville State", "Spread", "3-3-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("James Madison", "Pistol", "3-3-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Kansas", "Spread Option", "4-2-5 Man Pressure", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Kansas State", "Multiple", "4-2-5 Zone Pressure", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Kennesaw State", "Veer & Shoot", "3-3-5 Shell", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Kent State", "Spread", "Multiple", OffensivePlaybook.Spread, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Kentucky", "Spread", "4-2-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Liberty", "Spread Option", "3-4 Zone", OffensivePlaybook.Option, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("LA Lafayette", "Power Spread", "4-3", OffensivePlaybook.PowerRun, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.FourThree, OffensiveTempo.ChewClock),
        new("LA Tech", "Air Raid", "3-3-5 Man Pressure", OffensivePlaybook.AirRaid, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("Louisville", "Multiple", "4-2-5 Zone", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("LSU", "Veer & Shoot", "Multiple", OffensivePlaybook.Option, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Marshall", "Power Spread", "3-3-5", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Maryland", "Spread", "3-4 Man Pressure", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Memphis", "Veer & Shoot", "4-2-5 Man Pressure", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Miami", "Air Raid", "4-2-5 Zone Pressure", OffensivePlaybook.AirRaid, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("Miami OH", "Spread", "4-3 Zone", OffensivePlaybook.Spread, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("Michigan", "Power Spread", "4-3 Man Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.FourThree, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.FourThree, OffensiveTempo.ChewClock),
        new("Michigan State", "Multiple", "4-2-5 Zone Pressure", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Mid Tenn State", "Spread", "3-4 Man", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Minnesota", "Multiple", "4-2-5 Man Pressure", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Mississippi State", "Veer & Shoot", "3-3-5", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Missouri", "Power Spread", "4-2-5 Zone", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Missouri State", "Spread", "3-3-5 Tite", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Navy", "Option", "3-3-5 Zone Pressure", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("NC State", "Power Spread", "3-4 Shell", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.ProtectPass, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Nebraska", "Multiple", "4-2-5 Man Pressure", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Nevada", "Spread", "Multiple", OffensivePlaybook.Spread, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("New Mexico", "Multiple", "4-3 Zone", OffensivePlaybook.Balanced, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("New Mexico St", "Power Spread", "3-3-5 Shell", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("North Carolina", "Spread", "3-4 Multiple", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("North Dakota State", "Multiple", "4-3", OffensivePlaybook.Balanced, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("North Texas", "Spread", "3-3-5 Shell", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Northern Illinois", "Spread", "4-2-5 Shell", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Northwestern", "Spread", "4-2-5 Shell", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Notre Dame", "Power Spread", "Multiple", OffensivePlaybook.PowerRun, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Ohio", "Pistol", "4-2-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Ohio State", "Multiple", "4-2-5 Man Pressure", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Oklahoma", "Spread", "4-2-5 Man Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Oklahoma State", "Air Raid", "Multiple", OffensivePlaybook.AirRaid, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("Old Dominion", "Veer & Shoot", "3-2-6", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Ole Miss", "Veer & Shoot", "4-2-5", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Oregon", "Spread", "3-4 Shell", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Oregon State", "Multiple", "3-4 Zone", OffensivePlaybook.Balanced, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Penn State", "Power Spread", "3-4 Man", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.ProtectPass, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Pittsburgh", "Spread", "4-3 Press Quarters", OffensivePlaybook.Spread, DefensivePlaybook.FourThree, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("Purdue", "Air Raid", "Multiple", OffensivePlaybook.AirRaid, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("Rice", "Spread Option", "3-4 Man Pressure", OffensivePlaybook.Option, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.Pistol, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Rutgers", "Spread", "4-2-5 Shell", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Sacramento State", "Spread", "4-2-5 Zone", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Sam Houston", "Air Raid", "3-3-5 Man", OffensivePlaybook.AirRaid, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("San Diego State", "Spread", "4-2-5", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("San Jose State", "Run & Shoot", "4-3 Multiple", OffensivePlaybook.AirRaid, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.Empty, DefensiveFormation.FourThree, OffensiveTempo.UpTempo),
        new("Southern Methodist", "Spread", "4-2-5 Man Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("South Alabama", "Spread", "4-2-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("South Carolina", "Air Raid", "4-2-5 Man", OffensivePlaybook.AirRaid, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("Southern Miss", "Power Spread", "3-3-5 Zone Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Stanford", "Power Spread", "4-3 Zone", OffensivePlaybook.PowerRun, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.FourThree, OffensiveTempo.ChewClock),
        new("Syracuse", "Spread", "4-2-5", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("TCU", "Spread", "3-3-5 Zone", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Temple", "Spread", "3-4 Zone", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Tennessee", "Veer & Shoot", "4-2-5 Shell", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Texas", "Spread", "3-3-5 Tite", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Texas AM", "Power Spread", "4-2-5 Zone Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Texas State", "Veer & Shoot", "3-4 Zone Pressure", OffensivePlaybook.Option, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.Pistol, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Texas Tech", "Veer & Shoot", "3-4 Shell", OffensivePlaybook.Option, DefensivePlaybook.ThreeFour, DefensiveApproach.ProtectPass, OffensiveFormation.Pistol, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Toledo", "Spread", "4-2-5 Shell", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Troy", "Veer & Shoot", "3-3-5 Three High", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Tulane", "Power Spread", "3-3-5 Tite", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Tulsa", "Spread", "3-3-5 Shell", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("UAB", "Power Spread", "4-2-5 Man Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Central Florida", "Spread", "4-3 Multiple", OffensivePlaybook.Spread, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("UCLA", "Spread", "4-2-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("UConn", "Spread", "4-3 Press Quarters", OffensivePlaybook.Spread, DefensivePlaybook.FourThree, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("LA Monroe", "Spread", "3-3-5 Three High", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("UMass", "Spread", "4-2-5 Man", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("UNLV", "Spread", "4-2-5 Man Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("USC", "Air Raid", "4-2-5 Shell", OffensivePlaybook.AirRaid, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Empty, DefensiveFormation.Nickel, OffensiveTempo.UpTempo),
        new("South Florida", "Spread", "3-3-5 Tite", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Utah", "Spread", "4-3 Man Pressure", OffensivePlaybook.Spread, DefensivePlaybook.FourThree, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.FourThree, OffensiveTempo.Normal),
        new("Utah State", "Power Spread", "3-4 Zone Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("UT El Paso", "Spread", "3-3-5 Three High", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("UT San Antonio", "Spread", "3-4 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Vanderbilt", "Pistol", "4-2-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Virginia", "Power Spread", "4-2-5", OffensivePlaybook.PowerRun, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Virginia Tech", "Power Spread", "3-4 Zone Pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock),
        new("Wake Forest", "Spread", "4-2-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Washington", "Multiple", "3-4 Man Pressure", OffensivePlaybook.Balanced, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.ThreeFour, OffensiveTempo.Normal),
        new("Washington State", "Multiple", "Multiple", OffensivePlaybook.Balanced, DefensivePlaybook.Balanced, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("West Virginia", "Spread Option", "3-3-5 Zone Pressure", OffensivePlaybook.Option, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.Pistol, DefensiveFormation.Nickel, OffensiveTempo.ChewClock),
        new("Western Kentucky", "Air Raid", "3-4", OffensivePlaybook.AirRaid, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.Empty, DefensiveFormation.ThreeFour, OffensiveTempo.UpTempo),
        new("Western Michigan", "Spread", "4-2-5 Zone Pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.Normal),
        new("Wisconsin", "Power Spread", "4-3 Multiple", OffensivePlaybook.PowerRun, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.FourThree, OffensiveTempo.ChewClock),
        new("Wyoming", "Multiple", "4-2-5 Man", OffensivePlaybook.Balanced, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal), 
    ];
}
