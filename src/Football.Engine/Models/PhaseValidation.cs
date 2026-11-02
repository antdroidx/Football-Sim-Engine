namespace DynastyManager.Core.Models;

public sealed record PhaseBlocker(string Code, string Message, string ActionKey);
public sealed record PhaseValidation(IReadOnlyList<PhaseBlocker> Blockers)
{
    public bool CanAdvance => Blockers.Count == 0;
    public string? FirstReason => Blockers.FirstOrDefault()?.Message;
    public static PhaseValidation Ready { get; } = new([]);
    public static PhaseValidation Blocked(string code, string message, string actionKey) => new([new(code, message, actionKey)]);
}
public sealed record PhaseTask(string Id, string Title, string? Detail = null);
