using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>
/// Derived identity used by both games. Team game plans stay authoritative;
/// personnel shape adapts the current building identity only, not the saved plan.
/// </summary>
public sealed record TeamIdentity
{
    public required string TeamName { get; init; }
    public OffensivePlaybook Offense { get; init; }
    public DefensivePlaybook Defense { get; init; }
    public DefensiveApproach Approach { get; init; }
    public int PressureTendency { get; init; } = 50;
    public int ZoneOrProtectTendency { get; init; } = 50;
    public double TalentVersusFit { get; init; } = .55;
    public double DevelopmentalUpside { get; init; } = .35;
    public IReadOnlyDictionary<Position, int> PositionPriorities { get; init; } = new Dictionary<Position, int>();
    public StaffPhilosophy? StaffPhilosophy { get; init; }
}

/// <summary>
/// Optional head-staff philosophy that adapts identity weights without changing a saved team plan.
/// </summary>
public sealed record StaffPhilosophy(OffensivePlaybook Offense, DefensivePlaybook Defense, double Weight);


public sealed record PersonnelProfile(double PassRatio, double RunRatio, double CoverageRatio, double EdgeRatio);

/// <summary>
/// Deterministic identity-derived positional priorities and player values.
/// </summary>
public static class RosterPlanService
{
    public static TeamIdentity GetIdentity(
        DynastyState state,
        string team,
        IReadOnlyList<DynastyPlayer>? roster = null)
    {
        var plan = state.TeamGamePlans.LastOrDefault(item => item.TeamName.Equals(team, StringComparison.OrdinalIgnoreCase));
        var head = StaffManagementService.GetStaff(state, team, StaffRole.HeadCoach);
        var coaching = StaffAttributeService.Coaching(head);
        var offense = plan?.Offense ?? OffensivePlaybook.Balanced;
        var defense = plan?.Defense ?? DefensivePlaybook.Balanced;
        var approach = plan?.Approach ?? DefensiveApproach.Balanced;
        var shapes = PersonnelProfile(roster ?? RosterShapes(state, team));
        (offense, defense) = AdaptToPersonnel(offense, defense, shapes);
        var pressure = approach switch
        {
            DefensiveApproach.Pressure => .82,
            DefensiveApproach.ProtectPass => .48,
            DefensiveApproach.StopRun => .45,
            _ => .62
        };

        return new TeamIdentity
        {
            TeamName = team,
            Offense = offense,
            Defense = defense,
            Approach = approach,
            PressureTendency = (int)Math.Round(pressure * 100),
            ZoneOrProtectTendency = (int)Math.Round((1-pressure) * 100),
            TalentVersusFit = head is null ? .55 : Math.Clamp(.50 + (coaching.OffensePlaycalling - 65) * .005, .35, .75),
            DevelopmentalUpside = head is null ? .35 : Math.Clamp(.25 + (coaching.Development / 100d) * .25, .20, .55),
            PositionPriorities = GetPositionPriorities(offense, defense, approach)
        };
    }

    public static PersonnelProfile PersonnelProfile(IReadOnlyList<DynastyPlayer> roster)
    {
        return new(
            ShapeRatio(roster, Position.WR, Position.TE),
            ShapeRatio(roster, Position.RB, Position.FB, Position.OL),
            ShapeRatio(roster, Position.CB, Position.FS, Position.SS),
            ShapeRatio(roster, Position.DE, Position.OLB));
    }

    public static (OffensivePlaybook Offense, DefensivePlaybook Defense) AdaptToPersonnel(
        OffensivePlaybook offense, DefensivePlaybook defense, PersonnelProfile shapes)
    {
        if (shapes.PassRatio > shapes.RunRatio * 1.2 && shapes.PassRatio > 1.15 &&
            offense is OffensivePlaybook.PowerRun or OffensivePlaybook.Option or OffensivePlaybook.Flexbone)
            offense = OffensivePlaybook.Spread;
        else if (shapes.RunRatio > shapes.PassRatio * 1.2 && shapes.RunRatio > 1.15 &&
                 offense is OffensivePlaybook.AirRaid or OffensivePlaybook.Spread)
            offense = OffensivePlaybook.PowerRun;

        if (shapes.CoverageRatio > shapes.EdgeRatio * 1.2 && shapes.CoverageRatio > 1.15 &&
            defense is DefensivePlaybook.ThreeFour or DefensivePlaybook.FourThree)
            defense = DefensivePlaybook.Nickel;
        else if (shapes.EdgeRatio > shapes.CoverageRatio * 1.2 && shapes.EdgeRatio > 1.15 &&
                 defense is DefensivePlaybook.Nickel or DefensivePlaybook.FourThree)
            defense = DefensivePlaybook.ThreeFour;

        return (offense, defense);
    }

    /// <summary>
    /// Blends two identity priority dictionaries by staff-influence weight without changing either source.
    /// </summary>
    public static IReadOnlyDictionary<Position, int> BlendPriorities(
        IReadOnlyDictionary<Position, int> primary,
        IReadOnlyDictionary<Position, int> secondary,
        double weight)
    {
        if (weight <= 0 || secondary.Count == 0) return primary;
        if (weight >= 1) return secondary;
        return primary.ToDictionary(
            pair => pair.Key,
            pair => (int)Math.Round(pair.Value * (1 - weight) + secondary.GetValueOrDefault(pair.Key, pair.Value) * weight));
    }

    /// <summary>
    /// Blends a positional depth target toward the staff philosophy by weight.
    /// </summary>
    public static int BlendTarget(int primary, int secondary, double weight) =>
        weight <= 0 ? primary : weight >= 1 ? secondary : (int)Math.Round(primary * (1 - weight) + secondary * weight);
    private static IReadOnlyList<DynastyPlayer> RosterShapes(DynastyState state, string team)
    {
        return state.ActiveRoster
            .Where(item => item.TeamName.Equals(team, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static double ShapeRatio(IReadOnlyList<DynastyPlayer> roster, params Position[] positions)
    {
        var count = roster.Count(item => positions.Contains(item.Position));
        var target = positions.Sum(position => DynastyRosterRules.TargetPositionCounts.GetValueOrDefault(position, 1));
        return target <= 0 ? 0 : count / (double)target;
    }

    public static int GetRosterTarget(
        DynastyState state,
        Position position,
        string team,
        TeamIdentity? identity = null)
    {
        identity ??= GetIdentity(state, team);
        return position switch
        {
            Position.FB when identity.Offense is OffensivePlaybook.PowerRun or OffensivePlaybook.Flexbone => 1,
            Position.FB => 0,
            Position.QB => 3,
            Position.OL => 11,
            Position.RB => identity.Offense is OffensivePlaybook.PowerRun ? 5 : 4,
            Position.WR => identity.Offense switch
            {
                OffensivePlaybook.AirRaid => 9,
                OffensivePlaybook.Spread => 8,
                OffensivePlaybook.PowerRun or OffensivePlaybook.Option or OffensivePlaybook.Flexbone => 6,
                _ => 7
            },
            Position.TE => 2,
            Position.CB => identity.Defense == DefensivePlaybook.Nickel ? 6 : 5,
            Position.DE => identity.Defense == DefensivePlaybook.ThreeFour ? 3 : 4,
            Position.DT => 4,
            Position.OLB => identity.Defense == DefensivePlaybook.ThreeFour ? 4 : identity.Defense == DefensivePlaybook.Nickel ? 5 : 3,
            Position.MLB => identity.Defense == DefensivePlaybook.ThreeFour ? 3 : 4,
            Position.FS or Position.SS => 2,
            Position.K => 1,
            Position.P => identity.Offense is OffensivePlaybook.AirRaid or OffensivePlaybook.Spread ? 0 : 1,
            _ => 2
        };
    }

    public static IReadOnlyDictionary<Position, int> GetPositionPriorities(
        OffensivePlaybook offense, DefensivePlaybook defense, DefensiveApproach approach)
    {
        var priorities = Enum.GetValues<Position>().ToDictionary(position => position, _ => 1.0);
        priorities[Position.QB] = 1.8;
        priorities[Position.OL] = offense switch
        {
            OffensivePlaybook.AirRaid => 1.15,
            OffensivePlaybook.Spread => 1.14,
            OffensivePlaybook.PowerRun or OffensivePlaybook.Option or OffensivePlaybook.Flexbone => 1.30,
            OffensivePlaybook.PlayAction => 1.20,
            _ => 1.12
        };
        priorities[Position.WR] = offense switch
        {
            OffensivePlaybook.AirRaid => 1.32,
            OffensivePlaybook.Spread => 1.22,
            OffensivePlaybook.PlayAction => 1.10,
            OffensivePlaybook.PowerRun or OffensivePlaybook.Option or OffensivePlaybook.Flexbone => .88,
            _ => 1.05
        };
        priorities[Position.TE] = offense switch
        {
            OffensivePlaybook.AirRaid => .85,
            OffensivePlaybook.Spread => .95,
            OffensivePlaybook.PowerRun or OffensivePlaybook.Option or OffensivePlaybook.Flexbone => 1.22,
            _ => 1.05
        };
        priorities[Position.RB] = offense switch
        {
            OffensivePlaybook.AirRaid => .85,
            OffensivePlaybook.Spread => .95,
            OffensivePlaybook.PowerRun or OffensivePlaybook.Option or OffensivePlaybook.Flexbone => 1.28,
            _ => 1.05
        };
        priorities[Position.DE] = approach == DefensiveApproach.Pressure ? 1.35 : defense == DefensivePlaybook.ThreeFour ? 1.20 : 1.10;
        priorities[Position.DT] = approach == DefensiveApproach.StopRun ? 1.25 : defense == DefensivePlaybook.ThreeFour ? 1.10 : 1.18;
        priorities[Position.OLB] = approach == DefensiveApproach.Pressure ? 1.30 : defense == DefensivePlaybook.ThreeFour ? 1.22 : .98;
        priorities[Position.MLB] = approach == DefensiveApproach.StopRun ? 1.28 : defense == DefensivePlaybook.ThreeFour ? 1.16 : 1.08;
        priorities[Position.CB] = approach == DefensiveApproach.ProtectPass ? 1.25 : defense == DefensivePlaybook.Nickel ? 1.20 : 1.10;
        priorities[Position.FS] = priorities[Position.SS] = approach == DefensiveApproach.ProtectPass ? 1.18 : 1.05;
        priorities[Position.K] = .75;
        priorities[Position.P] = .75;
        return priorities.ToDictionary(item => item.Key, item => (int)Math.Round(item.Value * 100d));
    }

    public static double GetFit(DynastyState state, string team, DynastyPlayer player) =>
        Math.Clamp(DepthChartService.FitScore(state, team, player,
            Ncaa06RatingService.PositionCode(player)), 25, 99);

    public static double GetValue(DynastyState state, string team, DynastyPlayer player, string source = "roster")
    {
        var identity = GetIdentity(state, team);
        var players = state.ActiveRoster.Where(item => item.TeamName.Equals(team, StringComparison.OrdinalIgnoreCase)).ToArray();
        var atPosition = players.Where(item => item.Position == player.Position).ToArray();
        var target = GetRosterTarget(state, player.Position, team, identity);
        var starters = DynastyRosterRules.StarterPositionCounts.GetValueOrDefault(player.Position, 1);
        var starterGrade = atPosition.OrderByDescending(item => item.OverallRating)
            .ElementAtOrDefault(starters - 1)?.OverallRating ?? 55;
        var need = Math.Max(0, target - atPosition.Length);
        var upgrade = player.OverallRating - starterGrade;
        var fit = GetFit(state, team, player) / 100d;
        var positional = identity.PositionPriorities.TryGetValue(player.Position, out var weight) ? weight / 100d : 1;
        var young = player.ClassYear <= 2 ? identity.DevelopmentalUpside : source == "portal" ? .25 : -.25;
        var graduationRisk = source == "portal" && player.ClassYear >= 4 ? .8 : 1;
        return Math.Clamp(
            fit * positional * (45 + player.OverallRating) * .08 +
            positional * need * 5 +
            upgrade * .8 + young,
            1, 100) * graduationRisk;
    }

    /// <summary>
    /// College staff lean is derived from the head coach's stronger side, because college staff records carry
    /// numeric coaching attributes instead of scheme labels. Generated staff ratings differ by small offsets,
    /// so a lean of at least three marks a clearly offense- or defense-focused coach. Weight follows the same
    /// tenure/leadership rule as the Pro model. College acquisition scoring never reads this; it only powers
    /// CPU plan drift and audit evidence, so season-zero and balanced staff keep the catalog identity exactly.
    /// </summary>
    public static StaffPhilosophy? HeadLeanPhilosophy(
        DynastyState state, string team, OffensivePlaybook fallbackOffense, DefensivePlaybook fallbackDefense)
    {
        var head = StaffManagementService.GetStaff(state, team, StaffRole.HeadCoach);
        if (head is null) return null;
        return HeadLeanPhilosophy(head, StaffAttributeService.Coaching(head), fallbackOffense, fallbackDefense);
    }

    public static StaffPhilosophy? HeadLeanPhilosophy(
        StaffMember head, CoachingAttributes coaching, OffensivePlaybook fallbackOffense, DefensivePlaybook fallbackDefense)
    {
        var lean = coaching.OffensePlaycalling - coaching.DefenseScheme;
        var offense = lean >= 3 ? OffensivePlaybook.Spread
            : lean <= -3 ? OffensivePlaybook.PowerRun
            : (OffensivePlaybook?)null;
        if (offense is null) return null;
        var weight = Math.Clamp(.05 * head.TenureYears + (head.Leadership - 55) / 300d, .05, .40);
        return new(offense.Value, fallbackDefense, weight);
    }

    /// <summary>
    /// CPU plan drift starts once head-coach influence is established: roughly two seasons of tenure with
    /// average leadership. The saved team plan still keeps its authority because each step is one adjacent
    /// playbook per offseason toward the coach target, and user plans never drift.
    /// </summary>
    public const double MinimumDriftInfluence = .10;

    /// <summary>
    /// Bounded CPU plan drift. Each step moves a saved offense/defense one adjacency step toward the staff
    /// target; custom playbooks and plans already at the target stay unchanged. Shared by both games.
    /// </summary>
    public static TeamGamePlan DriftPlan(
        TeamGamePlan plan, OffensivePlaybook? offenseTarget, DefensivePlaybook? defenseTarget)
        => plan with
        {
            Offense = OffenseStep(plan.Offense, offenseTarget),
            Defense = DefenseStep(plan.Defense, defenseTarget)
        };

    public static OffensivePlaybook OffenseStep(OffensivePlaybook offense, OffensivePlaybook? target) =>
        target is not OffensivePlaybook goal || goal is OffensivePlaybook.Custom || offense is OffensivePlaybook.Custom
            ? offense : StepTowards(offense, goal, OffensePaths);

    public static DefensivePlaybook DefenseStep(DefensivePlaybook defense, DefensivePlaybook? target) =>
        target is not DefensivePlaybook goal || goal is DefensivePlaybook.Custom || defense is DefensivePlaybook.Custom
            ? defense : StepTowards(defense, goal, DefensePaths);

    static readonly IReadOnlyDictionary<OffensivePlaybook, IReadOnlyList<OffensivePlaybook>> OffensePaths = new
        Dictionary<OffensivePlaybook, IReadOnlyList<OffensivePlaybook>>
    {
        [OffensivePlaybook.Balanced] = [OffensivePlaybook.PlayAction, OffensivePlaybook.Spread, OffensivePlaybook.PowerRun],
        [OffensivePlaybook.PlayAction] = [OffensivePlaybook.Balanced, OffensivePlaybook.Spread],
        [OffensivePlaybook.Spread] = [OffensivePlaybook.Balanced, OffensivePlaybook.PlayAction, OffensivePlaybook.AirRaid, OffensivePlaybook.PowerRun],
        [OffensivePlaybook.AirRaid] = [OffensivePlaybook.Spread],
        [OffensivePlaybook.PowerRun] = [OffensivePlaybook.Balanced, OffensivePlaybook.Spread, OffensivePlaybook.Option],
        [OffensivePlaybook.Option] = [OffensivePlaybook.PowerRun, OffensivePlaybook.Spread, OffensivePlaybook.Flexbone],
        [OffensivePlaybook.Flexbone] = [OffensivePlaybook.Option]
    };

    static readonly IReadOnlyDictionary<DefensivePlaybook, IReadOnlyList<DefensivePlaybook>> DefensePaths = new
        Dictionary<DefensivePlaybook, IReadOnlyList<DefensivePlaybook>>
    {
        [DefensivePlaybook.Balanced] = [DefensivePlaybook.FourThree],
        [DefensivePlaybook.FourThree] = [DefensivePlaybook.Balanced, DefensivePlaybook.ThreeFour, DefensivePlaybook.Nickel],
        [DefensivePlaybook.ThreeFour] = [DefensivePlaybook.FourThree, DefensivePlaybook.Nickel],
        [DefensivePlaybook.Nickel] = [DefensivePlaybook.FourThree, DefensivePlaybook.ThreeFour]
    };

    static T StepTowards<T>(T current, T target, IReadOnlyDictionary<T, IReadOnlyList<T>> graph) where T : struct, Enum
    {
        if (EqualityComparer<T>.Default.Equals(current, target)) return current;
        T best = current; var bestDistance = int.MaxValue;
        foreach (var neighbor in NeighborsOf(current, graph).Keys)
        {
            var distance = DistanceFrom(neighbor, target, graph);
            if (distance < bestDistance) { best = neighbor; bestDistance = distance; }
        }
        return bestDistance is int.MaxValue ? current : best;
    }

    static IReadOnlyDictionary<T, bool> NeighborsOf<T>(T node, IReadOnlyDictionary<T, IReadOnlyList<T>> graph) where T : struct, Enum
    {
        var neighbors = new Dictionary<T, bool>();
        if (graph.TryGetValue(node, out var forward)) foreach (var item in forward) neighbors[item] = true;
        foreach (var (other, list) in graph) if (list.Contains(node)) neighbors.TryAdd(other, true);
        return neighbors;
    }

    static int DistanceFrom<T>(T from, T target, IReadOnlyDictionary<T, IReadOnlyList<T>> graph) where T : struct, Enum
    {
        if (EqualityComparer<T>.Default.Equals(from, target)) return 0;
        var seen = new HashSet<T> { from };
        var frontier = new List<T> { from };
        for (var depth = 1; frontier.Count > 0; depth++)
        {
            var next = new List<T>();
            foreach (var node in frontier) foreach (var neighbor in NeighborsOf(node, graph).Keys) if (seen.Add(neighbor)) next.Add(neighbor);
            if (next.Contains(target)) return depth;
            frontier = next;
        }
        return int.MaxValue;
    }

    /// <summary>
    /// College CPU game plans drift one step per offseason toward a head coach whose stronger coaching side
    /// diverges from the saved plan. User plans, plans without staff targets and weak influence stay unchanged.
    /// </summary>
    public static DynastyState ApplyCpuPlanDrift(DynastyState state)
    {
        var plans = state.TeamGamePlans.ToList();
        var changed = false;
        foreach (var team in state.LeagueTeams)
        {
            if (team.Name.Equals(state.UserTeamName, StringComparison.OrdinalIgnoreCase)) continue;
            var index = plans.FindIndex(plan => plan.TeamName.Equals(team.Name, StringComparison.OrdinalIgnoreCase));
            if (index < 0) continue;
            var philosophy = HeadLeanPhilosophy(state, team.Name, plans[index].Offense, plans[index].Defense);
            if (philosophy is null || philosophy.Weight < MinimumDriftInfluence) continue;
            var drifted = DriftPlan(plans[index], philosophy.Offense, null);
            if (drifted == plans[index]) continue;
            plans[index] = drifted;
            changed = true;
        }
        return changed ? state with { TeamGamePlans = plans.ToArray() } : state;
    }

    static double priorit(double value) => value;
}
