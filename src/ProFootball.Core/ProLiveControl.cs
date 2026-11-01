using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
namespace ProFootball.Core;

// Compatibility entry points delegate to the league-independent engine controller.
public static class ProLiveControl
{
    public static bool CanCall(GameDaySession session, bool defending) => new GameDayLiveControl(session).CanCall(defending);
    public static GameSnapEvent Step(GameDaySession session, string? offense, string? defense) => new GameDayLiveControl(session).Step(offense, defense);
}
