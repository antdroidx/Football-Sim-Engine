using DynastyManager.Core.Models;
using Microsoft.Maui.Graphics;

namespace DynastyManager.App;

/// <summary>The only application-specific inputs to the common live-game screen.</summary>
public sealed record GameCastLiveBrand(string Abbreviation, string Primary, string Secondary, string? FieldName = null);

public sealed class GameCastLiveHost
{
    public required Func<string, GameCastLiveBrand> Brand { get; init; }
    public required Func<GameDaySession, Task> Save { get; init; }
    public required Func<INavigation, GameDayBoxScore, Task> ShowBoxScore { get; init; }
    public Func<string, IDrawable?> Logo { get; init; } = _ => null;
    public Func<GameDaySession, GameFieldStyle>? FieldStyle { get; init; }
    public bool ChooseTeamOnTakeover { get; init; }
    // Each application already has its own preference store; these keys are shared by all engine hosts.
    public string PreferencesPrefix { get; init; } = "gamecast";
}
