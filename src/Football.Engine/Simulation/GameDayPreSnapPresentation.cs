using System;
using System.Threading;
using System.Threading.Tasks;

namespace DynastyManager.Core.Simulation;

/// <summary>Presentation-only ordering for the interval between a called play and its snap.</summary>
public static class GameDayPreSnapPresentation
{
    public static readonly TimeSpan ActionLead = TimeSpan.FromMilliseconds(180);
    public static readonly TimeSpan SetHold = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan FormationLead = TimeSpan.FromSeconds(3.5);
    public static readonly TimeSpan LiveMotionLead = TimeSpan.FromSeconds(1.6);

    public static Task HoldAsync(
        bool hasAlignmentAction,
        Func<Task> showFormation,
        Func<Task> animateAlignment,
        Func<TimeSpan, CancellationToken, Task> delay,
        CancellationToken cancellationToken = default,
        bool settleAfterAlignment = true) =>
        HoldSequenceAsync(
            hasAlignmentAction,
            showFormation,
            null,
            animateAlignment,
            delay,
            cancellationToken,
            settleAfterAlignment);

    public static Task HoldSequenceAsync(
        bool hasAlignmentAction,
        Func<Task> showFormation,
        Func<Task>? animateDefensiveAlignment,
        Func<Task> animateOffensiveAlignment,
        Func<TimeSpan, CancellationToken, Task> delay,
        CancellationToken cancellationToken = default,
        bool settleAfterAlignment = true,
        bool motionLiveAtSnap = false)
    {
        ArgumentNullException.ThrowIfNull(showFormation);
        ArgumentNullException.ThrowIfNull(animateOffensiveAlignment);
        ArgumentNullException.ThrowIfNull(delay);
        return CoreAsync();

        async Task CoreAsync()
        {
            await showFormation();

            // The formation lead is wall-clock time from lineup visibility. Alignment
            // animation may consume part of it, but never shortens the minimum lead.
            var formationLead = delay(FormationLead, cancellationToken);
            if (hasAlignmentAction || animateDefensiveAlignment is not null)
                await delay(ActionLead, cancellationToken);
            if (animateDefensiveAlignment is not null)
                await animateDefensiveAlignment();

            if (hasAlignmentAction)
            {
                if (motionLiveAtSnap)
                {
                    await delay(LiveMotionLead, cancellationToken);
                    await animateOffensiveAlignment();
                }
                else
                {
                    await animateOffensiveAlignment();
                    if (settleAfterAlignment) await delay(SetHold, cancellationToken);
                }
            }

            await formationLead;
        }
    }
}
