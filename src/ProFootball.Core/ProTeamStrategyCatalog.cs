using DynastyManager.Core.Models;
namespace ProFootball.Core;

/// <summary>
/// Season-zero franchise identities. Scheme descriptions are best-effort 2026 coaching profiles; identities come
/// from the packaged official team-site snapshot. Quality is a staff-identity index for the simulation, not a
/// retroactive performance grade. It is centered around 70 so the league starts balanced and then evolves.
/// </summary>
public sealed record ProTeamStrategy(
    string TeamId,
    string TeamName,
    string OffenseIdentity,
    string DefenseIdentity,
    OffensivePlaybook Offense,
    DefensivePlaybook Defense,
    DefensiveApproach Approach,
    OffensiveFormation OffensiveFormation,
    DefensiveFormation DefensiveFormation,
    OffensiveTempo Tempo,
    int CoachingQuality,
    int ScoutingQuality)
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

public static class ProTeamStrategyCatalog
{
    static readonly Lazy<IReadOnlyDictionary<string, ProTeamStrategy>> IdIndex =
        new(() => Teams!.ToDictionary(team => team.TeamId, StringComparer.OrdinalIgnoreCase));
    static readonly Lazy<IReadOnlyDictionary<string, ProTeamStrategy>> NameIndex =
        new(() => Teams!.ToDictionary(team => team.TeamName, StringComparer.OrdinalIgnoreCase));
    static IReadOnlyDictionary<string, ProTeamStrategy> ById => IdIndex.Value;
    static IReadOnlyDictionary<string, ProTeamStrategy> ByName => NameIndex.Value;
    public static IReadOnlyList<ProTeamStrategy> All => Teams;
    public static ProTeamStrategy Get(string teamIdOrName) =>
        ById.GetValueOrDefault(teamIdOrName) ?? ByName.GetValueOrDefault(teamIdOrName) ??
        throw new ArgumentException("No Pro team strategy is available.", nameof(teamIdOrName));
    public static bool TryGet(string teamIdOrName, out ProTeamStrategy strategy) =>
        (ById.TryGetValue(teamIdOrName, out strategy!) || ByName.TryGetValue(teamIdOrName, out strategy!));

    public static CoachingAttributes Attributes(ProCoach coach, Guid? franchiseId = null)
    {
        if (coach.Coaching is not null) return coach.Coaching;
        var team = Get(coach.TeamId);
        int Rating(string key, int adjustment = 0) => Math.Clamp(
            team.CoachingQuality + adjustment + Noise($"{franchiseId:N}|{coach.TeamId}", coach.Role, key, 4), 50, 88);
        var generated=coach.Role switch
        {
            StaffRole.HeadCoach => new CoachingAttributes
            {
                Leadership=Rating("lead", 4), Development=Rating("development", 2),
                OffensePlaycalling=Rating("offense", 1), OffenseDevelopment=Rating("off-development", 2),
                DefenseScheme=Rating("defense", 1), DefenseDevelopment=Rating("def-development", 2),
                SpecialTeams=Rating("special-teams", -3), TalentEvaluation=Rating("talent", 3)
            },
            StaffRole.OffensiveCoordinator => new CoachingAttributes
            {
                Leadership=Rating("lead", -3), Development=Rating("development", 1),
                OffensePlaycalling=Rating("offense", 5), OffenseDevelopment=Rating("off-development", 5),
                DefenseScheme=Rating("defense", -5), DefenseDevelopment=Rating("def-development", -3),
                SpecialTeams=Rating("special-teams", -4), TalentEvaluation=Rating("talent", -2)
            },
            StaffRole.DefensiveCoordinator => new CoachingAttributes
            {
                Leadership=Rating("lead", -2), Development=Rating("development", 1),
                OffensePlaycalling=Rating("offense", -4), OffenseDevelopment=Rating("off-development", -3),
                DefenseScheme=Rating("defense", 5), DefenseDevelopment=Rating("def-development", 5),
                SpecialTeams=Rating("special-teams", -4), TalentEvaluation=Rating("talent", -1)
            },
            StaffRole.SpecialTeamsCoordinator => new CoachingAttributes
            {
                Leadership=Rating("lead", -2), Development=Rating("development", -1),
                OffensePlaycalling=Rating("offense", -5), OffenseDevelopment=Rating("off-development", -4),
                DefenseScheme=Rating("defense", -5), DefenseDevelopment=Rating("def-development", -4),
                SpecialTeams=Rating("special-teams", 7), TalentEvaluation=Rating("talent", -2)
            },
            _ => new CoachingAttributes
            {
                Leadership=Rating("lead"), Development=Rating("development"),
                OffensePlaycalling=Rating("offense"), OffenseDevelopment=Rating("off-development"),
                DefenseScheme=Rating("defense"), DefenseDevelopment=Rating("def-development"),
                SpecialTeams=Rating("special-teams"), TalentEvaluation=Rating("talent")
            }
        };
        // Preserve non-default skills from legacy saves/imports during the additive migration.
        return generated with
        {
            Leadership=coach.Leadership != 55 ? coach.Leadership : generated.Leadership,
            Development=coach.Development != 65 ? coach.Development : generated.Development,
            OffensePlaycalling=coach.OffenseRating != 55 ? coach.OffenseRating : generated.OffensePlaycalling,
            DefenseScheme=coach.DefenseRating != 55 ? coach.DefenseRating : generated.DefenseScheme,
            SpecialTeams=coach.SpecialTeamsRating != 55 ? coach.SpecialTeamsRating : generated.SpecialTeams
        };
    }

    public static int CoachingTalentEvaluation(ProFranchise state, string team)
    {
        var club = state.League.Teams.Single(item => item.Name == team);
        var values = state.Coaches.Where(c => c.TeamId == club.Id && !c.Vacant).ToArray();
        if (values.Length == 0) return Get(club.Id).CoachingQuality;
        double Weight(StaffRole role) => role switch
        {
            StaffRole.HeadCoach => .50, StaffRole.OffensiveCoordinator => .20,
            StaffRole.DefensiveCoordinator => .20, StaffRole.SpecialTeamsCoordinator => .10, _ => 0
        };
        var total = values.Sum(c => Weight(c.Role));
        return total <= 0 ? Get(club.Id).CoachingQuality :
            (int)Math.Round(values.Sum(c => Attributes(c).TalentEvaluation * Weight(c.Role)) / total);
    }

    public static ProFranchise Ensure(ProFranchise state)
    {
        var plans = state.GamePlans.ToList();
        var existing = plans.Select(plan => plan.TeamName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var team in state.League.Teams)
        {
            if (existing.Contains(team.Name) || !TryGet(team.Id, out var identity)) continue;
            plans.Add(identity.GamePlan);
            existing.Add(team.Name);
        }
        var coaches = state.Coaches.Select(coach =>
        {
            var identity = TryGet(coach.TeamId, out var value) ? value : null;
            var attributes = Attributes(coach, state.Id);
            var preferredOffense = identity?.OffenseIdentity ?? "Balanced";
            var preferredDefense = identity?.DefenseIdentity ?? "Balanced";
            var staff = ProStaff.InitializeMetadata(coach, state.Id);
            return ProStaff.Apply(staff, attributes) with
            {
                PreferredOffense = string.IsNullOrWhiteSpace(coach.PreferredOffense) || coach.PreferredOffense == "Balanced" ? preferredOffense : coach.PreferredOffense,
                PreferredDefense = string.IsNullOrWhiteSpace(coach.PreferredDefense) || coach.PreferredDefense == "Balanced" ? preferredDefense : coach.PreferredDefense
            };
        }).ToList();
        return state with { GamePlans = plans, Coaches = coaches };
    }

    static int Noise(string team, StaffRole role, string attribute, int radius)
    {
        unchecked
        {
            var hash = 2166136261u;
            foreach (var ch in $"{team}|{role}|{attribute}".ToUpperInvariant()) hash = (hash ^ ch) * 16777619u;
            return (int)(hash % (uint)(radius * 2 + 1)) - radius;
        }
    }

    static readonly ProTeamStrategy[] Teams =
    [
        new("ARI", "Arizona Cardinals", "Spread", "3-4 Multiple", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.UpTempo, 66, 65),
        new("ATL", "Atlanta Falcons", "Play-action balanced", "Multiple", OffensivePlaybook.PlayAction, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal, 68, 68),
        new("BAL", "Baltimore Ravens", "Power run play action", "Multiple pressure", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock, 75, 75),
        new("BUF", "Buffalo Bills", "Spread play action", "Nickel pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.UpTempo, 74, 72),
        new("CAR", "Carolina Panthers", "Power spread", "Hybrid 3-4", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock, 64, 64),
        new("CHI", "Chicago Bears", "Play-action timing", "4-3 hybrid", OffensivePlaybook.PlayAction, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal, 69, 68),
        new("CIN", "Cincinnati Bengals", "Spread timing", "Nickel coverage", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.ProtectPass, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.UpTempo, 70, 67),
        new("CLE", "Cleveland Browns", "Zone run play action", "Multiple 3-4", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock, 65, 64),
        new("DAL", "Dallas Cowboys", "Play-action balanced", "Multiple 4-3", OffensivePlaybook.PlayAction, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal, 68, 68),
        new("DEN", "Denver Broncos", "Play-action spread concepts", "Multiple pressure", OffensivePlaybook.PlayAction, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.ThreeFour, OffensiveTempo.Normal, 73, 72),
        new("DET", "Detroit Lions", "Play-action power", "4-3 aggressive", OffensivePlaybook.PlayAction, DefensivePlaybook.FourThree, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal, 75, 73),
        new("GB", "Green Bay Packers", "West Coast play action", "4-3 quarters", OffensivePlaybook.PlayAction, DefensivePlaybook.FourThree, DefensiveApproach.ProtectPass, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal, 74, 74),
        new("HOU", "Houston Texans", "Play-action balanced", "4-3 pressure", OffensivePlaybook.PlayAction, DefensivePlaybook.FourThree, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal, 74, 70),
        new("IND", "Indianapolis Colts", "Spread play action", "Hybrid multiple", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.UpTempo, 69, 69),
        new("JAX", "Jacksonville Jaguars", "Spread play action", "Multiple pressure", OffensivePlaybook.Spread, DefensivePlaybook.Nickel, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.Nickel, OffensiveTempo.UpTempo, 67, 68),
        new("KC", "Kansas City Chiefs", "Spread motion", "3-4 pressure", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.UpTempo, 77, 74),
        new("LAC", "Los Angeles Chargers", "Power run play action", "3-4 multiple", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock, 72, 69),
        new("LAR", "Los Angeles Rams", "Wide-zone play action", "3-4 hybrid", OffensivePlaybook.PlayAction, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal, 75, 74),
        new("LV", "Las Vegas Raiders", "Play-action balanced", "Multiple fronts", OffensivePlaybook.PlayAction, DefensivePlaybook.Nickel, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.Nickel, OffensiveTempo.Normal, 65, 63),
        new("MIA", "Miami Dolphins", "Timing spread", "4-3 multiple", OffensivePlaybook.Spread, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.FourThree, OffensiveTempo.UpTempo, 68, 68),
        new("MIN", "Minnesota Vikings", "Play-action motion", "3-4 pressure", OffensivePlaybook.PlayAction, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.ThreeFour, OffensiveTempo.Normal, 74, 70),
        new("NE", "New England Patriots", "Erhardt-Perkins multiple", "3-4 multiple", OffensivePlaybook.Balanced, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.ThreeFour, OffensiveTempo.Normal, 67, 67),
        new("NO", "New Orleans Saints", "Spread play action", "3-4 multiple", OffensivePlaybook.Spread, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.ShotgunSpread, DefensiveFormation.ThreeFour, OffensiveTempo.UpTempo, 66, 66),
        new("NYG", "New York Giants", "Multiple play action", "Multiple fronts", OffensivePlaybook.Balanced, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.FourThree, OffensiveTempo.Normal, 67, 65),
        new("NYJ", "New York Jets", "Balanced play action", "4-3 multiple", OffensivePlaybook.Balanced, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.Pistol, DefensiveFormation.FourThree, OffensiveTempo.Normal, 65, 64),
        new("PHI", "Philadelphia Eagles", "RPO and play-action spread", "4-3 pressure", OffensivePlaybook.PlayAction, DefensivePlaybook.FourThree, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal, 77, 78),
        new("PIT", "Pittsburgh Steelers", "Physical play action", "3-4 pressure", OffensivePlaybook.Balanced, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.Pistol, DefensiveFormation.ThreeFour, OffensiveTempo.Normal, 72, 72),
        new("SEA", "Seattle Seahawks", "Play-action balanced", "4-3", OffensivePlaybook.PlayAction, DefensivePlaybook.FourThree, DefensiveApproach.Balanced, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal, 73, 72),
        new("SF", "San Francisco 49ers", "Wide-zone play action", "4-3 pressure", OffensivePlaybook.PlayAction, DefensivePlaybook.FourThree, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.FourThree, OffensiveTempo.Normal, 76, 76),
        new("TB", "Tampa Bay Buccaneers", "Play-action spread", "3-4 pressure", OffensivePlaybook.PlayAction, DefensivePlaybook.ThreeFour, DefensiveApproach.Pressure, OffensiveFormation.Singleback, DefensiveFormation.ThreeFour, OffensiveTempo.Normal, 72, 70),
        new("TEN", "Tennessee Titans", "Power run play action", "3-4 multiple", OffensivePlaybook.PowerRun, DefensivePlaybook.ThreeFour, DefensiveApproach.Balanced, OffensiveFormation.IForm, DefensiveFormation.ThreeFour, OffensiveTempo.ChewClock, 63, 62),
        new("WAS", "Washington Commanders", "Spread RPO", "4-3 pressure", OffensivePlaybook.Spread, DefensivePlaybook.FourThree, DefensiveApproach.Pressure, OffensiveFormation.ShotgunSpread, DefensiveFormation.FourThree, OffensiveTempo.UpTempo, 71, 69), 
    ];
}
