using System;
using System.Threading;
using System.Threading.Tasks;

namespace DynastyManager.Core.Simulation;

/// <summary>Presentation-only ordering for the interval between a called play and its snap.</summary>
public static class GameDayPreSnapPresentation
{
    public static readonly TimeSpan ActionLead = TimeSpan.FromMilliseconds(180);
    public static readonly TimeSpan FormationHold = TimeSpan.FromSeconds(1);

    public static Task HoldAsync(
        bool hasAlignmentAction,
        Func<Task> showFormation,
        Func<Task> animateAlignment,
        Func<TimeSpan, CancellationToken, Task> delay,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(showFormation);
        ArgumentNullException.ThrowIfNull(animateAlignment);
        ArgumentNullException.ThrowIfNull(delay);
        return CoreAsync();

        async Task CoreAsync()
        {
            await showFormation();
            if (!hasAlignmentAction)
            {
                await delay(FormationHold, cancellationToken);
                return;
            }

            await delay(ActionLead, cancellationToken);
            await animateAlignment();
            await delay(FormationHold, cancellationToken);
        }
    }
}
