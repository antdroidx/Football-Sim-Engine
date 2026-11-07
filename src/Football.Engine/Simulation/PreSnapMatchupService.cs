using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public enum PreSnapCoverageReaction
{
    ManMatchupHeld,
    ZonePassOff,
    FocusedMatchupHeld
}

/// <summary>
/// Resolves how a defense reacts to a receiver's pre-snap movement. Man and focused
/// matchups carry the same defender through motion; zone coverage passes the receiver
/// off to the defender responsible for the new space.
/// </summary>
public sealed record PreSnapMatchup(
    PreSnapCoverageReaction Reaction,
    bool DefenderFollowsMotion,
    bool MatchupChanged,
    double SeparationAdjustment,
    double PlayAdjustment,
    PreSnapEffect? Effect);

public static class PreSnapMatchupService
{
    public static bool IsParticipant(
        PreSnapChoice choice,
        IReadOnlyList<DynastyPlayer> players,
        DynastyPlayer player)
    {
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(player);
        return choice.Type switch
        {
            PreSnapType.Shift => PreSnapMotionPlan.SelectShiftPrimary(choice, players)?.PlayerId == player.PlayerId,
            PreSnapType.Motion =>
                PreSnapMotionPlan.SelectPrimary(choice, players)?.PlayerId == player.PlayerId ||
                PreSnapMotionPlan.SelectSwapPartner(choice, players)?.PlayerId == player.PlayerId,
            _ => false
        };
    }

    public static bool IsFocusedMatchup(
        DefensivePlayDefinition defense,
        Position receiverPosition,
        int receiverRole)
    {
        ArgumentNullException.ThrowIfNull(defense);
        if (defense.Assignments.Any(assignment =>
            assignment.Task == AssignmentTask.Man &&
            assignment.Position == receiverPosition &&
            assignment.RoleIndex == receiverRole))
            return true;

        // The bracket call is the built-in way to tell the secondary to carry a
        // primary outside receiver through motion instead of passing him off.
        return receiverPosition == Position.WR && receiverRole < 2 &&
            defense.Name.Contains("Bracket", StringComparison.OrdinalIgnoreCase);
    }

    public static bool DefenderFollowsMotion(
        DefensivePlayDefinition defense,
        PlayAssignment? defenderAssignment,
        Position receiverPosition,
        int receiverRole,
        bool receiverIsMotionParticipant)
    {
        ArgumentNullException.ThrowIfNull(defense);
        return receiverIsMotionParticipant &&
            (!LiveFootballChoreography.Zone(defense, defenderAssignment) ||
             IsFocusedMatchup(defense, receiverPosition, receiverRole));
    }

    public static PreSnapMatchup? Resolve(
        PreSnapChoice? choice,
        DefensivePlayDefinition defense,
        Position receiverPosition,
        int receiverRole,
        PlayAssignment? defenderAssignment,
        bool receiverIsMotionParticipant,
        bool isPass,
        int receiverAbility,
        int defenderAbility)
    {
        ArgumentNullException.ThrowIfNull(defense);
        if (choice is not { HasAlignmentAction: true } || !receiverIsMotionParticipant)
            return null;

        var focused = IsFocusedMatchup(defense, receiverPosition, receiverRole);
        var zone = LiveFootballChoreography.Zone(defense, defenderAssignment);
        var followsMotion = choice.Type == PreSnapType.Motion && (!zone || focused);
        var hasAlignmentMovement = choice.Type is PreSnapType.Motion or PreSnapType.Shift;
        var matchupChanged = hasAlignmentMovement && !followsMotion && zone && !focused;
        var reaction = focused ? PreSnapCoverageReaction.FocusedMatchupHeld :
            zone ? PreSnapCoverageReaction.ZonePassOff : PreSnapCoverageReaction.ManMatchupHeld;

        var abilityDelta = receiverAbility - defenderAbility;
        var travel = Math.Clamp(choice.Yards / 3d, 0, 1);
        var swapLoad = choice.MotionKind == PreSnapMotionKind.SwapAlignments ? .08 : 0;
        var separation = reaction switch
        {
            PreSnapCoverageReaction.FocusedMatchupHeld => -.25 + abilityDelta * .008,
            PreSnapCoverageReaction.ManMatchupHeld => .08 + abilityDelta * .012,
            _ => .22 + abilityDelta * .020 + travel * .12 + swapLoad
        };
        if (choice.Type == PreSnapType.Shift) separation *= .60;
        separation = Math.Clamp(separation, -.9, 1.4);

        var yardEffect = isPass
            ? separation >= .70 ? 2 : separation >= .20 ? 1 : separation <= -.70 ? -2 : separation <= -.20 ? -1 : 0
            : reaction == PreSnapCoverageReaction.ZonePassOff
                ? separation >= .25 ? 1 : separation <= -.35 ? -1 : 0
                : reaction == PreSnapCoverageReaction.FocusedMatchupHeld ? -1 : 0;

        var note = yardEffect switch
        {
            > 0 => reaction == PreSnapCoverageReaction.ZonePassOff
                ? $"The motion changed the zone matchup and added {yardEffect} {(yardEffect == 1 ? "yard" : "yards")}."
                : $"The motion held its matchup and added {yardEffect} {(yardEffect == 1 ? "yard" : "yards")}.",
            < 0 => $"The defense answered the motion and cost the offense {Math.Abs(yardEffect)} {(yardEffect == -1 ? "yard" : "yards")}.",
            _ => reaction switch
            {
                PreSnapCoverageReaction.ZonePassOff => "The motion changed the zone matchup without a material yard change.",
                PreSnapCoverageReaction.FocusedMatchupHeld => "The focused coverage carried the motion and held the matchup.",
                _ => "Man coverage followed the motion; the matchup held."
            }
        };

        return new(
            reaction,
            followsMotion,
            matchupChanged,
            separation,
            Math.Clamp(separation * 2.5, -3, 3),
            new(yardEffect, note));
    }
}
