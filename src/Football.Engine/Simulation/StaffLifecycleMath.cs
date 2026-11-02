using DynastyManager.Core;
using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>League-neutral staff arithmetic. Hiring, retirement and development policies stay in each league.</summary>
public static class StaffLifecycleMath
{
    public static int Shift(int roll, int growthChance, int declineChance) =>
        roll < growthChance ? 1 : roll >= 100 - declineChance ? -1 : 0;

    public static int Attribute(int current, int shift, int minimum = 35, int maximum = 96) =>
        Math.Clamp(current + shift, minimum, maximum);
}
