using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class KickoffPolicy
{
    public static int TouchbackSpot(FootballRules rules, bool touchedLandingZone, int fromOwnYardLine = 35) =>
        rules.DynamicKickoff && (touchedLandingZone || fromOwnYardLine == 50) ? 20 : rules.KickoffTouchbackYardLine;

    public static double MovementProgress(double progress, bool dynamicKickoff, bool kicker = false) =>
        dynamicKickoff && !kicker ? Math.Clamp((progress - .65) / .35, 0, 1) : progress;

    public static int AlignmentYard(bool receiving, bool specialist, int slot, int landing,
        int kickStart = 65, bool dynamicKickoff = false, bool onside = false)
    {
        if (onside) return receiving ? kickStart - 10 - slot % 2 * 3 : specialist ? kickStart + 5 : kickStart;
        if (!dynamicKickoff) return receiving ? specialist ? landing : slot < 5 ? 43 : 25 : specialist ? kickStart + 5 : kickStart;
        if (!receiving) return specialist ? kickStart + 5 : 40;
        if (specialist) return Math.Min(20, landing);
        return slot is 1 or 3 or 5 or 6 or 8 or 10 ? 35 : 32;
    }
}
