using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Shared play-aware motion selection, snap alignment and presentation paths.</summary>
public static class PreSnapMotionPlan
{
    public static int RoleIndex(IReadOnlyList<DynastyPlayer> players, Position position, Guid playerId) =>
        players.TakeWhile(player => player.PlayerId != playerId).Count(player => player.Position == position);

    public static bool IsRole(PreSnapChoice choice, Position position, int roleIndex, bool swapRole = false)
    {
        var selectedPosition = swapRole ? choice.SwapPosition : choice.MotionPosition;
        var selectedRole = swapRole ? choice.SwapRoleIndex : choice.MotionRoleIndex;
        return selectedPosition == position && selectedRole == roleIndex;
    }

    public static DynastyPlayer? SelectPrimary(PreSnapChoice choice, IReadOnlyList<DynastyPlayer> players)
    {
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(players);
        if (choice.Type != PreSnapType.Motion) return null;
        if (choice.MotionPosition is Position position)
            return players.FirstOrDefault(player => player.Position == position &&
                RoleIndex(players, position, player.PlayerId) == choice.MotionRoleIndex);

        return players
            .Where(player => player.Position is Position.WR or Position.TE or Position.RB or Position.FB)
            .OrderBy(player => player.Position switch
            {
                Position.WR => 0,
                Position.TE => 1,
                Position.RB => 2,
                _ => 3
            })
            .ThenBy(player => player.PlayerId)
            .FirstOrDefault();
    }

    public static DynastyPlayer? SelectShiftPrimary(PreSnapChoice choice, IReadOnlyList<DynastyPlayer> players)
    {
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(players);
        if (choice.Type != PreSnapType.Shift) return null;
        if (choice.ShiftPosition is Position position)
            return players.FirstOrDefault(player => player.Position == position &&
                RoleIndex(players, position, player.PlayerId) == choice.ShiftRoleIndex);

        return players.FirstOrDefault(player => player.Position == Position.WR) ??
            players.FirstOrDefault(player => player.Position is Position.TE or Position.RB or Position.FB);
    }

    public static DynastyPlayer? SelectSwapPartner(PreSnapChoice choice, IReadOnlyList<DynastyPlayer> players)
    {
        if (choice.Type != PreSnapType.Motion || choice.MotionKind != PreSnapMotionKind.SwapAlignments ||
            choice.SwapPosition is not Position position) return null;
        return players.FirstOrDefault(player => player.Position == position &&
            RoleIndex(players, position, player.PlayerId) == choice.SwapRoleIndex);
    }

    public static bool IsPrimary(PreSnapChoice choice, IReadOnlyList<DynastyPlayer> players, DynastyPlayer? player) =>
        player is not null && SelectPrimary(choice, players)?.PlayerId == player.PlayerId;

    public static bool IsSwapPartner(PreSnapChoice choice, IReadOnlyList<DynastyPlayer> players, DynastyPlayer? player) =>
        player is not null && SelectSwapPartner(choice, players)?.PlayerId == player.PlayerId;

    /// <summary>Position at the snap after the play-specific motion finishes.</summary>
    public static GameFieldPoint SnapAlignment(
        PreSnapChoice choice,
        bool primary,
        bool swapPartner,
        GameFieldPoint baseAlignment,
        GameFieldPoint otherAlignment,
        GameFieldPoint quarterbackAlignment)
    {
        if (choice.Type != PreSnapType.Motion) return baseAlignment;
        if (choice.MotionKind == PreSnapMotionKind.SwapAlignments)
            return primary ? otherAlignment : swapPartner ? otherAlignment : baseAlignment;
        if (!primary) return baseAlignment;

        return choice.MotionKind switch
        {
            PreSnapMotionKind.JetHandoff => new(
                quarterbackAlignment.X - Math.Max(.25, Math.Abs(choice.SnapDepth)),
                Math.Clamp(choice.SnapLane ?? quarterbackAlignment.Y +
                    (choice.Side.Equals("Right", StringComparison.OrdinalIgnoreCase) ? .08 : -.08), .08, .92)),
            PreSnapMotionKind.SetAcross or PreSnapMotionKind.OrbitAcross => new(
                choice.SnapDepth,
                Math.Clamp(choice.SnapLane ?? 1 - baseAlignment.Y, .08, .92)),
            _ => baseAlignment
        };
    }

    /// <summary>Final alignment for a receiver shift. A shift changes stance/alignment without live motion.</summary>
    public static GameFieldPoint ShiftAlignment(PreSnapChoice choice, GameFieldPoint baseAlignment)
    {
        if (choice.Type != PreSnapType.Shift) return baseAlignment;
        var yards = Math.Max(0, choice.Yards);
        var lane = choice.Side.Equals("Right", StringComparison.OrdinalIgnoreCase) ? yards : -yards;
        return baseAlignment with { Y = Math.Clamp(baseAlignment.Y + lane / 53.333, .06, .94) };
    }

    /// <summary>Path used by the live pre-snap animation. The final point is the snap alignment.</summary>
    public static IReadOnlyList<GameFieldPoint> Path(
        PreSnapChoice choice,
        GameFieldPoint baseAlignment,
        GameFieldPoint snapAlignment,
        GameFieldPoint quarterbackAlignment,
        double fieldWidthYards = 53.333)
    {
        if (choice.Type != PreSnapType.Motion) return [baseAlignment];
        var side = choice.Side.Equals("Right", StringComparison.OrdinalIgnoreCase) ? 1 : -1;
        return choice.MotionKind switch
        {
            PreSnapMotionKind.JetHandoff =>
            [
                baseAlignment,
                new((baseAlignment.X + snapAlignment.X) * .55, baseAlignment.Y + (snapAlignment.Y - baseAlignment.Y) * .12),
                new(snapAlignment.X - .20, snapAlignment.Y),
                snapAlignment
            ],
            PreSnapMotionKind.SetAcross =>
            [
                baseAlignment,
                new(snapAlignment.X, baseAlignment.Y),
                snapAlignment
            ],
            PreSnapMotionKind.OrbitAcross =>
            [
                baseAlignment,
                new(baseAlignment.X - 2.5, baseAlignment.Y + side * .04),
                new((baseAlignment.X + snapAlignment.X) * .55, .50),
                snapAlignment
            ],
            PreSnapMotionKind.SwapAlignments =>
            [
                baseAlignment,
                new((baseAlignment.X + snapAlignment.X) * .5, (baseAlignment.Y + snapAlignment.Y) * .5),
                snapAlignment
            ],
            _ =>
            [
                baseAlignment,
                new(baseAlignment.X, Math.Clamp(baseAlignment.Y + side * choice.Yards / Math.Max(1, fieldWidthYards), .05, .95)),
                baseAlignment
            ]
        };
    }
}
