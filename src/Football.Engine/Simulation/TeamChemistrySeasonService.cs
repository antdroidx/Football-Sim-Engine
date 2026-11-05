using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Season inputs a host derives from its own recorded state before the calendar rolls over.</summary>
public sealed record TeamChemistryInputs
{
    public required string TeamName { get; init; }
    public required int SeasonYear { get; init; }
    public required int EvaluatedCohesion { get; init; }
    public required int EvaluatedSchemeFamiliarity { get; init; }
    public int RosterTurnoverPercent { get; init; }
    public string HeadCoachKey { get; init; } = "";
    public string PlanKey { get; init; } = "";
    public int MoraleSignal { get; init; }
    public int DisciplineSignal { get; init; }
    public string DisciplineNote { get; init; } = "Discipline issues cost the room.";
    public string MoraleNote { get; init; } = "Morale finished the season strong.";
    public Guid[] CaptainIds { get; init; } = [];
    public string[] CaptainNames { get; init; } = [];
}

/// <summary>
/// Turns the previous season's culture record into the next season's baseline. Every adjustment is bounded and
/// explained, and the same inputs always produce the same record.
/// </summary>
public static class TeamChemistrySeasonService
{
    public const int Minimum = 20;
    public const int Maximum = 95;

    public static TeamChemistryRecord Advance(TeamChemistryRecord? prior, TeamChemistryInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var notes = new List<string>();
        var captains = inputs.CaptainIds.Distinct().ToArray();
        var evaluated = Clamp(inputs.EvaluatedCohesion);
        var evaluatedScheme = Clamp(inputs.EvaluatedSchemeFamiliarity);

        if (prior is null)
        {
            notes.Add("First recorded season. Culture values come from the current roster and staff.");
            return new TeamChemistryRecord
            {
                TeamId = StableId(inputs.TeamName, inputs.SeasonYear),
                TeamName = inputs.TeamName,
                SeasonYear = inputs.SeasonYear,
                Cohesion = evaluated,
                SchemeFamiliarity = evaluatedScheme,
                EvaluatedCohesion = evaluated,
                EvaluatedSchemeFamiliarity = evaluatedScheme,
                Momentum = CohesionMomentum.Stable,
                Captains = captains,
                CaptainNames = inputs.CaptainNames,
                RosterTurnoverPercent = Math.Clamp(inputs.RosterTurnoverPercent, 0, 100),
                HeadCoachKey = inputs.HeadCoachKey,
                PlanKey = inputs.PlanKey,
                Notes = notes.ToArray()
            };
        }

        var coachChanged = Changed(prior.HeadCoachKey, inputs.HeadCoachKey);
        var planChanged = Changed(prior.PlanKey, inputs.PlanKey);

        double cohesion = prior.Cohesion + (evaluated - prior.Cohesion) * 0.35;
        double scheme = prior.SchemeFamiliarity + (evaluatedScheme - prior.SchemeFamiliarity) * 0.30;

        var turnover = Math.Clamp(inputs.RosterTurnoverPercent, 0, 100);
        if (turnover > 0)
        {
            cohesion -= turnover * 0.25;
            notes.Add($"Roster turnover of {turnover}% reset continuity.");
        }

        if (coachChanged)
        {
            cohesion -= 4;
            notes.Add("A new head coach is rebuilding the room.");
        }
        else
        {
            notes.Add("Head coach continuity carried over.");
        }

        if (planChanged)
        {
            scheme = Math.Min(scheme, evaluatedScheme) - 6;
            cohesion -= 2;
            notes.Add("A changed game plan cut into scheme familiarity.");
        }
        else
        {
            scheme += 2;
            notes.Add("The same game plan carried over.");
        }

        if (prior.Captains.Length > 0)
        {
            var retained = captains.Count(id => prior.Captains.Contains(id));
            if (retained == 0)
            {
                cohesion -= 3;
                notes.Add("No captains returned from last season.");
            }
            else
            {
                cohesion += 2;
                notes.Add(retained == 1
                    ? "One captain returned to lead the room."
                    : $"{retained} captains returned to lead the room.");
            }
        }

        if (inputs.DisciplineSignal != 0)
        {
            cohesion += inputs.DisciplineSignal * 0.30;
            notes.Add(inputs.DisciplineSignal < 0 ? inputs.DisciplineNote : "No discipline problems were reported.");
        }

        if (inputs.MoraleSignal != 0)
        {
            cohesion += inputs.MoraleSignal * 0.15;
            notes.Add(inputs.MoraleSignal > 0 ? inputs.MoraleNote : "Morale finished the season low.");
        }

        cohesion = Clamp(cohesion);
        scheme = Clamp(scheme);
        var finalCohesion = (int)Math.Round(cohesion, MidpointRounding.AwayFromZero);
        var finalScheme = (int)Math.Round(scheme, MidpointRounding.AwayFromZero);

        return new TeamChemistryRecord
        {
            TeamId = StableId(inputs.TeamName, inputs.SeasonYear),
            TeamName = inputs.TeamName,
            SeasonYear = inputs.SeasonYear,
            Cohesion = finalCohesion,
            SchemeFamiliarity = finalScheme,
            EvaluatedCohesion = evaluated,
            EvaluatedSchemeFamiliarity = evaluatedScheme,
            Momentum = finalCohesion > prior.Cohesion
                ? CohesionMomentum.Rising
                : finalCohesion < prior.Cohesion
                    ? CohesionMomentum.Falling
                    : CohesionMomentum.Stable,
            Captains = captains,
            CaptainNames = inputs.CaptainNames,
            RosterTurnoverPercent = turnover,
            HeadCoachKey = inputs.HeadCoachKey,
            PlanKey = inputs.PlanKey,
            Notes = notes.ToArray()
        };
    }

    /// <summary>Keeps the season's culture offset while letting in-season form still move the live value.</summary>
    public static int ApplySeasonMemory(TeamChemistryRecord? record, int liveEvaluated) =>
        record is null
            ? Math.Clamp(liveEvaluated, Minimum, Maximum)
            : Math.Clamp(liveEvaluated + (record.Cohesion - record.EvaluatedCohesion), Minimum, Maximum);

    /// <summary>Keeps the season's scheme-familiarity offset while letting in-season form still move the live value.</summary>
    public static int ApplySchemeMemory(TeamChemistryRecord? record, int liveEvaluated) =>
        record is null
            ? Math.Clamp(liveEvaluated, Minimum, Maximum)
            : Math.Clamp(liveEvaluated + (record.SchemeFamiliarity - record.EvaluatedSchemeFamiliarity), Minimum, Maximum);

    public static string MomentumLabel(CohesionMomentum momentum) => momentum switch
    {
        CohesionMomentum.Rising => "Rising",
        CohesionMomentum.Falling => "Falling",
        _ => "Stable"
    };

    private static bool Changed(string prior, string current) =>
        !string.IsNullOrEmpty(prior) && !string.IsNullOrEmpty(current) &&
        !prior.Equals(current, StringComparison.Ordinal);

    private static int Clamp(int value) => Math.Clamp(value, Minimum, Maximum);

    private static double Clamp(double value) => Math.Clamp(value, Minimum, Maximum);

    private static Guid StableId(string seed, int seasonYear)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{seed}:{seasonYear}"));
        return new Guid(bytes.AsSpan(0, 16));
    }
}