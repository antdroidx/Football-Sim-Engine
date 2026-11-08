using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>Shared play-aware motion selection, collision-free snap alignment and presentation paths.</summary>
public static class PreSnapMotionPlan
{
    public const int MotionSegmentMilliseconds = 260;

    public static int RoleIndex(IReadOnlyList<DynastyPlayer> players, Position position, Guid playerId) =>
        players.TakeWhile(player => player.PlayerId != playerId).Count(player => player.Position == position);

    public static int RoleIndexAfterMotion(
        PreSnapChoice choice,
        IReadOnlyList<DynastyPlayer> players,
        DynastyPlayer player)
    {
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(player);
        var baseRole = RoleIndex(players, player.Position, player.PlayerId);
        if (choice.Type != PreSnapType.Motion || choice.MotionKind != PreSnapMotionKind.SwapAlignments)
            return baseRole;
        if (SelectPrimary(choice, players)?.PlayerId == player.PlayerId) return choice.SwapRoleIndex;
        if (SelectSwapPartner(choice, players)?.PlayerId == player.PlayerId) return choice.MotionRoleIndex;
        return baseRole;
    }

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

    /// <summary>Selects the outside or slot player implied by a generic receiver motion.</summary>
    public static (Position Position, int Role) MotionPlayerRole(OffensivePlayDefinition play, string destination)
    {
        ArgumentNullException.ThrowIfNull(play);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        var receivers = FormationAlignments(play)
            .Where(item => item.Position == Position.WR)
            .OrderBy(item => item.Point.Y)
            .ToArray();
        if (receivers.Length > 0 && destination.Equals("Left", StringComparison.OrdinalIgnoreCase))
            return (Position.WR, receivers[^1].Role);
        if (receivers.Length > 0 && destination.Equals("Right", StringComparison.OrdinalIgnoreCase))
            return (Position.WR, receivers[0].Role);

        if (receivers.Length >= 3)
        {
            var slot = receivers
                .Skip(1)
                .Take(receivers.Length - 2)
                .OrderBy(item => Math.Abs(item.Point.Y - .5))
                .First();
            return (Position.WR, slot.Role);
        }

        var insideReceiver = FormationAlignments(play)
            .Where(item => item.Position is Position.WR or Position.TE)
            .OrderBy(item => Math.Abs(item.Point.Y - .5))
            .FirstOrDefault();
        return insideReceiver.Position == default
            ? (Position.WR, 0)
            : (insideReceiver.Position, insideReceiver.Role);
    }

    /// <summary>Returns a formation-aware target that does not overlap another offensive player.</summary>
    public static GameFieldPoint OpenAlignment(
        OffensivePlayDefinition play,
        string destination,
        Position movingPosition,
        int movingRole)
    {
        ArgumentNullException.ThrowIfNull(play);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        var occupied = FormationAlignments(play)
            .Where(item => item.Position != movingPosition || item.Role != movingRole)
            .Select(item => item.Point)
            .ToArray();
        var moving = FormationAlignments(play)
            .FirstOrDefault(item => item.Position == movingPosition && item.Role == movingRole).Point;
        var line = FormationAlignments(play)
            .Where(item => item.Position == Position.OL)
            .Select(item => item.Point.Y)
            .ToArray();
        var lineMin = line.Length == 0 ? .34 : line.Min();
        var lineMax = line.Length == 0 ? .66 : line.Max();
        var candidates = DestinationCandidates(destination, moving, lineMin, lineMax);
        foreach (var candidate in candidates)
            if (IsOpen(candidate, movingPosition, movingRole, play)) return candidate;
        return candidates.OrderByDescending(point => Clearance(point, occupied)).First();
    }

    public static bool IsOpenAlignment(
        OffensivePlayDefinition play,
        GameFieldPoint target,
        Position movingPosition,
        int movingRole) =>
        IsOpen(target, movingPosition, movingRole, play);

    /// <summary>Share of the Jet path the motion man has covered when the ball is snapped.</summary>
    public const double JetTraveledAtSnap = .90;

    /// <summary>Lateral offset of the handoff from the quarterback's lane.</summary>
    public const double HandoffLaneOffset = .07;

    /// <summary>Point on the Jet path a given share of the way from the receiver to the quarterback.</summary>
    public static GameFieldPoint JetPoint(GameFieldPoint baseAlignment, GameFieldPoint handoffAlignment, double traveled) =>
        new(baseAlignment.X + (handoffAlignment.X - baseAlignment.X) * traveled,
            baseAlignment.Y + (handoffAlignment.Y - baseAlignment.Y) * traveled);

    /// <summary>
    /// Point where the quarterback hands the ball to the motion man. The receiver arrives at the
    /// quarterback's hip rather than behind him, so the exchange reads as a handoff.
    /// </summary>
    public static GameFieldPoint HandoffAlignment(PreSnapChoice choice, GameFieldPoint quarterbackAlignment)
    {
        if (choice.MotionKind != PreSnapMotionKind.JetHandoff) return quarterbackAlignment;
        var lane = quarterbackAlignment.Y + (choice.Side.Equals("Right", StringComparison.OrdinalIgnoreCase)
            ? -HandoffLaneOffset : HandoffLaneOffset);
        return new(quarterbackAlignment.X, Math.Clamp(lane, .08, .92));
    }

    /// <summary>
    /// Waypoints of the live Jet path. The path only moves toward the quarterback, and the last
    /// point is where the receiver stands when the ball is snapped.
    /// </summary>
    public static IReadOnlyList<GameFieldPoint> JetPath(
        GameFieldPoint baseAlignment,
        GameFieldPoint handoffAlignment,
        bool stopBeforeHandoff = true) =>
    [
        baseAlignment,
        JetPoint(baseAlignment, handoffAlignment, .40),
        JetPoint(baseAlignment, handoffAlignment, .70),
        stopBeforeHandoff
            ? JetPoint(baseAlignment, handoffAlignment, JetTraveledAtSnap)
            : handoffAlignment
    ];

    /// <summary>Point the motion man occupies when the ball is snapped.</summary>
    public static GameFieldPoint HandoffStartAlignment(
        PreSnapChoice choice,
        GameFieldPoint baseAlignment,
        GameFieldPoint snapAlignment)
    {
        if (choice.MotionKind != PreSnapMotionKind.JetHandoff) return snapAlignment;
        return JetPoint(baseAlignment, snapAlignment, JetTraveledAtSnap);
    }

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
            PreSnapMotionKind.JetHandoff => HandoffAlignment(choice, quarterbackAlignment),
            PreSnapMotionKind.SetAcross or PreSnapMotionKind.OrbitAcross or PreSnapMotionKind.ReceiverRelocate => new(
                choice.SnapDepth,
                Math.Clamp(choice.SnapLane ?? 1 - baseAlignment.Y, .08, .92)),
            _ => baseAlignment
        };
    }

    /// <summary>Position for one player after the selected shift or motion.</summary>
    public static GameFieldPoint SnapAlignment(
        PreSnapChoice choice,
        IReadOnlyList<DynastyPlayer> players,
        OffensivePlayDefinition play,
        DynastyPlayer player)
    {
        ArgumentNullException.ThrowIfNull(choice);
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(play);
        ArgumentNullException.ThrowIfNull(player);
        var role = RoleIndex(players, player.Position, player.PlayerId);
        var baseAlignment = GameDayAnimationPlanner.OffensiveAlignment(play, player.Position, role);
        if (choice.Type == PreSnapType.Shift)
            return SelectShiftPrimary(choice, players)?.PlayerId == player.PlayerId
                ? ShiftAlignment(choice, baseAlignment)
                : baseAlignment;
        if (choice.Type != PreSnapType.Motion) return baseAlignment;

        var primary = SelectPrimary(choice, players)?.PlayerId == player.PlayerId;
        var swapPartner = SelectSwapPartner(choice, players)?.PlayerId == player.PlayerId;
        if (primary && choice.MotionKind == PreSnapMotionKind.ReceiverRelocate && choice.SnapLane is null)
            return OpenAlignment(play, choice.Side, player.Position, role);
        var otherAlignment = baseAlignment;
        if (primary && choice.SwapPosition is Position swapPosition)
            otherAlignment = GameDayAnimationPlanner.OffensiveAlignment(play, swapPosition, choice.SwapRoleIndex);
        else if (swapPartner && choice.MotionPosition is Position motionPosition)
            otherAlignment = GameDayAnimationPlanner.OffensiveAlignment(play, motionPosition, choice.MotionRoleIndex);
        return SnapAlignment(
            choice,
            primary,
            swapPartner,
            baseAlignment,
            otherAlignment,
            GameDayAnimationPlanner.OffensiveAlignment(play, Position.QB, 0));
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
        double fieldWidthYards = 53.333,
        bool stopBeforeHandoff = false)
    {
        if (choice.Type != PreSnapType.Motion) return [baseAlignment];
        var side = choice.Side.Equals("Right", StringComparison.OrdinalIgnoreCase) ? 1 : -1;
        return choice.MotionKind switch
        {
            PreSnapMotionKind.JetHandoff => JetPath(baseAlignment, snapAlignment, stopBeforeHandoff),
            PreSnapMotionKind.SetAcross or PreSnapMotionKind.ReceiverRelocate =>
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

    static IEnumerable<(Position Position, int Role, GameFieldPoint Point)> FormationAlignments(OffensivePlayDefinition play)
    {
        yield return (Position.QB, 0, GameDayAnimationPlanner.OffensiveAlignment(play, Position.QB, 0));
        for (var role = 0; role < 5; role++)
            yield return (Position.OL, role, GameDayAnimationPlanner.OffensiveAlignment(play, Position.OL, role));

        var counts = new Dictionary<Position, int>();
        foreach (var position in GameDayAnimationPlanner.EligiblePositions(play.Personnel))
        {
            var role = counts.GetValueOrDefault(position);
            counts[position] = role + 1;
            yield return (position, role, GameDayAnimationPlanner.OffensiveAlignment(play, position, role));
        }
    }

    static IReadOnlyList<GameFieldPoint> DestinationCandidates(
        string destination,
        GameFieldPoint moving,
        double lineMin,
        double lineMax)
    {
        if (destination.Equals("Slot", StringComparison.OrdinalIgnoreCase))
        {
            var right = moving.Y < .5;
            return right
                ? [new(-1.5, lineMax + .14), new(-2.5, lineMax + .10), new(-1.5, lineMax + .22),
                    new(-3, lineMax + .12), new(-2, lineMax + .18), new(-2.5, lineMax + .24)]
                : [new(-1.5, lineMin - .14), new(-2.5, lineMin - .10), new(-1.5, lineMin - .22),
                    new(-3, lineMin - .12), new(-2, lineMin - .18), new(-2.5, lineMin - .24)];
        }

        var left = destination.Equals("Left", StringComparison.OrdinalIgnoreCase);
        return left
            ? [new(-2.5, lineMin - .06), new(-1.5, lineMin - .14), new(-3, lineMin - .08),
                new(-2, lineMin - .20), new(-1.5, lineMin + .06), new(-2.5, lineMin + .04),
                new(-3.5, lineMin - .16)]
            : [new(-2.5, lineMax + .06), new(-1.5, lineMax + .14), new(-3, lineMax + .08),
                new(-2, lineMax + .20), new(-1.5, lineMax - .06), new(-2.5, lineMax - .04),
                new(-3.5, lineMax + .16)];
    }

    static bool IsOpen(GameFieldPoint target, Position movingPosition, int movingRole, OffensivePlayDefinition play) =>
        FormationAlignments(play)
            .Where(item => item.Position != movingPosition || item.Role != movingRole)
            .All(item => !Overlaps(target, item.Point));

    static bool Overlaps(GameFieldPoint first, GameFieldPoint second) =>
        Math.Abs(first.X - second.X) < .80 && Math.Abs(first.Y - second.Y) < .075;

    static double Clearance(GameFieldPoint target, IReadOnlyList<GameFieldPoint> occupied) =>
        occupied.Count == 0 ? 10 : occupied.Min(point =>
            Math.Abs(target.X - point.X) / .80 + Math.Abs(target.Y - point.Y) / .075);
}
