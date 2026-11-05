using DynastyManager.Core.Models;

namespace DynastyManager.Core.Simulation;

/// <summary>League-independent control ownership and playback state. UI hosts never decide who advances a snap.</summary>
public sealed class GameDayLiveControl(GameDaySession session)
{
    public GameDaySession Session { get; } = session;
    public bool IsPaused { get; private set; }
    public bool IsBusy { get; private set; }
    int suspensions;
    public bool IsSuspended => suspensions > 0;
    public bool HasTeam => Session.ControlledTeamName == Session.Game.HomeTeamName || Session.ControlledTeamName == Session.Game.AwayTeamName;
    public bool Permitted(bool defense) => HasTeam && (defense ? Session.UserCoachRole != StaffRole.OffensiveCoordinator : Session.UserCoachRole != StaffRole.DefensiveCoordinator);
    public bool Manual(bool defense) => Session.Mode == GameDayMode.Play && Permitted(defense) && !(defense ? Session.DelegateDefense : Session.DelegateOffense);
    public bool CanCall(bool defense) => !Session.IsComplete && !Session.KickoffPending && Manual(defense)
        && (!defense || !Session.TryPending) && (Session.PossessionTeamName == Session.ControlledTeamName) != defense;
    public bool NeedsCall => CanCall(false) || CanCall(true);
    public bool ShouldAdvance => !Session.IsComplete && !IsPaused && !IsBusy && !IsSuspended && !NeedsCall;
    public string Ownership(bool defense) => !Permitted(defense) ? "CPU · role locked" : Manual(defense) ? "You" : "CPU";
    public string CallerLabel(bool defense) => (defense ? "Defense: " : "Offense: ") + Ownership(defense);
    public bool CanChoosePreSnap => CanCall(false) && !Session.TryPending;
    public string? Restriction(bool defense) => Permitted(defense) ? null : !HasTeam
        ? "Choose a participating team before changing play callers."
        : $"Your career role is {UserCoachControlService.Label(Session.UserCoachRole)}. You call {(defense ? "offense" : "defense")}; the CPU coordinator calls {(defense ? "defense" : "offense")}. Head-coach careers can call both units.";
    public string Status => Session.IsComplete ? "Final" : IsBusy ? IsPaused ? "Pausing after this play" : "Play in progress"
        : IsSuspended ? "Menu open · game held" : IsPaused ? "Paused" : NeedsCall ? "Waiting for your call" : "CPU playing";
    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;
    public void SetManual(bool defense, bool manual)
    {
        if (!Permitted(defense)) throw new InvalidOperationException("Your coaching role does not control this unit.");
        // Leaving Watch preserves the other unit as CPU until explicitly reclaimed.
        if (Session.Mode == GameDayMode.Watch) Session.DelegateOffense = Session.DelegateDefense = true;
        Session.Mode = GameDayMode.Play;
        if (defense) Session.DelegateDefense = !manual; else Session.DelegateOffense = !manual;
    }
    public void Watch() { Session.Mode = GameDayMode.Watch; Resume(); }
    public void TakeOver()
    {
        Session.Mode = GameDayMode.Play;
        Session.DelegateOffense = !Permitted(false); Session.DelegateDefense = !Permitted(true);
        Resume();
    }
    public IDisposable Suspend()
    {
        suspensions++;
        return new Suspension(() => suspensions--);
    }
    sealed class Suspension(Action release) : IDisposable
    {
        bool disposed;
        public void Dispose() { if (!disposed) { disposed = true; release(); } }
    }
    public bool TryBegin(bool manual)
    {
        if (Session.IsComplete || IsBusy || IsSuspended || (manual ? !NeedsCall : !ShouldAdvance)) return false;
        IsBusy = true; return true;
    }
    public void End() => IsBusy = false;
    public GameSnapEvent Step(string? offense, string? defense, PreSnapChoice? preSnap = null)
    {
        var offensiveChoice = CanCall(false) && preSnap is { HasAlignmentAction: true } && !Session.TryPending ? preSnap : null;
        return GameDayEngine.Step(Session, CanCall(false) ? offense : null, CanCall(true) ? defense : null, offensiveChoice);
    }

    public IReadOnlyList<string> SpecialCalls()
    {
        if (!CanCall(false)) return [];
        if (Session.TryPending) return !Session.Rules.TimedOvertime && Session.OvertimeRound >= 2
            ? ["special-two-point"] : ["special-extra-point", "special-two-point"];
        return Session.OvertimeRound == 0 || Session.Rules.TimedOvertime
            ? ["special-punt", "special-field-goal", "special-kneel", "special-spike"]
            : ["special-punt", "special-field-goal"];
    }
}
