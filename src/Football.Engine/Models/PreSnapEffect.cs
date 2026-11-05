namespace DynastyManager.Core.Models;

/// <summary>The bounded yard effect a pre-snap alignment action produced on the recorded play.</summary>
public sealed record PreSnapEffect(int Yards, string Note);