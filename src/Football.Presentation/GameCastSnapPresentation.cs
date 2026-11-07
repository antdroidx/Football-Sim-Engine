using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace DynastyManager.App;

public sealed record GameCastSnapPresentationContext
{
    public required GameDaySession Session { get; init; }
    public required GameSnapEvent Snap { get; init; }
    public required Border Highlight { get; init; }
    public required Label HighlightText { get; init; }
    public required Func<string, Color> TeamColor { get; init; }
    public required Func<GameSnapEvent, bool, Task> Animate { get; init; }
    public required Func<GameSnapEvent, Task> Replay { get; init; }
    public bool AutoReplay { get; init; }
}

/// <summary>Common snap presentation: animation, highlight banners, auto replay and period break signal.</summary>
public static class GameCastSnapPresentation
{
    public static async Task<bool> PresentAsync(GameCastSnapPresentationContext context)
    {
        var session = context.Session; var snap = context.Snap;
        if (!snap.IsPreSnapPenalty) await context.Animate(snap, false);
        await GameCastHighlight.ShowAsync(context.Highlight, context.HighlightText, session, snap, context.TeamColor);
        if (context.AutoReplay && GameDayLivePresentation.InstantReplay(snap)) await context.Replay(snap);
        var period = GameDayAnimationPlanner.PeriodHeadline(snap.Quarter, session.Quarter, session.IsComplete);
        if (period is not null) await GameCastHighlight.ShowAsync(context.Highlight, context.HighlightText, session, snap, context.TeamColor, period);
        return snap.Quarter < session.Quarter || snap.TimeoutTeamName is not null;
    }
}