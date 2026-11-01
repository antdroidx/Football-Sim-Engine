using Microsoft.Maui.Controls;

namespace DynastyManager.App;

public sealed record GameCastLiveAction(string Label, Func<Task> Invoke);

/// <summary>Canonical live-game action order shared by both apps.</summary>
public static class GameCastLiveMenu
{
    public static IReadOnlyList<GameCastLiveAction> Build(
        Func<Task>? depthChart = null,
        Func<Task>? boxScore = null,
        Func<Task>? weather = null,
        Func<Task>? offenseControl = null,
        Func<Task>? defenseControl = null,
        string offenseControlLabel = "Offense control",
        string defenseControlLabel = "Defense control",
        Func<Task>? coachTalk = null,
        Func<Task>? instantReplay = null,
        Func<Task>? viewingOptions = null,
        string? modeActionLabel = null,
        Func<Task>? modeAction = null,
        Func<Task>? finish = null,
        Func<Task>? exit = null)
    {
        var actions = new List<GameCastLiveAction>();
        void Add(string label, Func<Task>? action) { if (action is not null) actions.Add(new(label, action)); }
        Add("Depth chart & substitutions", depthChart);
        Add("Box score", boxScore);
        Add("Weather", weather);
        Add(offenseControlLabel, offenseControl);
        Add(defenseControlLabel, defenseControl);
        Add("Coach talk", coachTalk);
        Add("Instant replay", instantReplay);
        Add("Viewing options", viewingOptions);
        if (modeActionLabel is not null) Add(modeActionLabel, modeAction);
        Add("Finish game", finish);
        Add("Return to game menu", exit);
        return actions;
    }

    public static async Task InvokeAsync(Page page, string title, IReadOnlyList<GameCastLiveAction> actions)
    {
        if (actions.Count == 0) return;
        var pick = await GameDialogs.ActionSheetAsync(page, title, "Cancel", null, actions.Select(a => a.Label).ToArray());
        var action = actions.FirstOrDefault(a => a.Label == pick);
        if (action is not null) await action.Invoke();
    }
}