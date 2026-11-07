using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Presentation-only defensive alignment adjustment for meaningful offensive formation changes.</summary>
public static class PreSnapDefensivePlan
{
    public static IReadOnlyDictionary<Guid, GameFieldPoint> Alignments(
        IReadOnlyList<DynastyPlayer> defenders,
        DefensivePlayDefinition defensivePlay,
        OffensivePlayDefinition offensivePlay)
    {
        ArgumentNullException.ThrowIfNull(defenders);
        ArgumentNullException.ThrowIfNull(defensivePlay);
        ArgumentNullException.ThrowIfNull(offensivePlay);

        var starts = DefensivePursuitService.Alignments(defenders, defensivePlay);
        var strength = FormationStrength(offensivePlay);
        var meaningfulStrength = Math.Abs(strength) >= .35;
        var towardStrength = meaningfulStrength
            ? Math.Sign(strength) * .028
            : 0;

        var extremeSpread = offensivePlay.Formation == OffensiveFormation.Empty;
        var heavy = offensivePlay.Formation is OffensiveFormation.IForm or OffensiveFormation.Flexbone ||
            offensivePlay.Personnel is OffensivePersonnel.TwentyOne;
        var result = new Dictionary<Guid, GameFieldPoint>();

        foreach (var defender in defenders)
        {
            var start = starts[defender.PlayerId];
            var lateral = defender.Position switch
            {
                Position.DE or Position.DT => towardStrength * .75,
                Position.MLB or Position.OLB => towardStrength,
                Position.CB => towardStrength * .55,
                Position.FS or Position.SS => towardStrength * 1.15,
                _ => 0
            };
            var depth = defender.Position switch
            {
                Position.DE or Position.DT => extremeSpread ? .20 : heavy ? -.15 : 0,
                Position.MLB or Position.OLB => extremeSpread ? .55 : heavy ? -.30 : 0,
                Position.CB => extremeSpread ? .75 : 0,
                Position.FS or Position.SS => extremeSpread ? -.65 : heavy ? -.45 : 0,
                _ => 0
            };

            result[defender.PlayerId] = new(
                Math.Clamp(start.X + depth, -.5, 18),
                Math.Clamp(start.Y + lateral, .08, .92));
        }

        return result;
    }

    static double FormationStrength(OffensivePlayDefinition play)
    {
        var positions = GameDayAnimationPlanner.EligiblePositions(play.Personnel);
        var strength = 0.0;
        for (var role = 0; role < positions.Count; role++)
        {
            var position = positions[role];
            var positionRole = 0;
            for (var prior = 0; prior < role; prior++)
                if (positions[prior] == position) positionRole++;

            var lane = GameDayAnimationPlanner.OffensiveAlignment(play, position, positionRole).Y;
            strength += Math.Sign(lane - .5) * (position == Position.TE ? 1.5 : 1);
        }

        return Math.Clamp(strength / Math.Max(1, positions.Count), -1, 1);
    }
}
