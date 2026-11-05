namespace DynastyManager.Core.Simulation;

public enum PreSnapType { None, Shift, Motion }

/// <summary>Compact pre-snap intent used only by presentation and call planning for this slice.</summary>
public sealed record PreSnapChoice(PreSnapType Type, string Side, double Yards)
{
    public bool Valid => PreSnapCatalog.IsValid(this);
    public bool HasAlignmentAction => Type is not PreSnapType.None;
}
