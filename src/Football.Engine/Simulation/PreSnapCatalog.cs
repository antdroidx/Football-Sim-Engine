using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

public static class PreSnapCatalog
{
    public static IReadOnlyList<PreSnapChoice> Common =>
    [
        new(PreSnapType.None, "None", 0),
        new(PreSnapType.Shift, "Left", .5),
        new(PreSnapType.Shift, "Right", .5),
        new(PreSnapType.Motion, "Left", 2),
        new(PreSnapType.Motion, "Right", 2)
    ];

    public static PreSnapChoice Motion(string side, double yards) =>
        new(PreSnapType.Motion, side, yards) { };

    public static bool IsValid(PreSnapChoice choice) => choice.Type switch
    {
        PreSnapType.None => choice.Yards == 0,
        PreSnapType.Shift => choice.Yards >= 0 && choice.Yards <= 1,
        PreSnapType.Motion => choice.Yards >= 0 && choice.Yards <= 3,
        _ => false
    };
}
