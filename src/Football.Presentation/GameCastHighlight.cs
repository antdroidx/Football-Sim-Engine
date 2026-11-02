using DynastyManager.Core.Models;
using DynastyManager.Core.Simulation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace DynastyManager.App;

/// <summary>Shared touchdown, timeout and period banner used by both live viewers.</summary>
public static class GameCastHighlight
{
    public static async Task ShowAsync(Border highlight, Label text, GameDaySession session, GameSnapEvent snap,
        Func<string, Color> teamColor, string? period = null)
    {
        var game = session.Game;
        var headline = period ?? (snap.IsSafety ? "SAFETY"
            : snap.IsTouchback
            ? $"TOUCHBACK\nBall at {(snap.BallEndYardLine > 0 ? snap.BallEndYardLine : snap.Outcome == SnapOutcome.Kickoff ? session.Rules.KickoffTouchbackYardLine : 20)}"
            : snap.Points >= 6 ? "TOUCHDOWN"
            : snap.Outcome == SnapOutcome.ClockStoppage
                ? snap.TimeoutTeamName is not null ? "TIMEOUT" : snap.Description?.Contains("Two-minute", StringComparison.OrdinalIgnoreCase) == true ? "TWO-MINUTE TIMEOUT" : null
                : GameDayAnimationPlanner.OutcomeHeadline(snap.Outcome));
        if (headline is null) return;
        var penalty = GameDayCommentaryService.Penalty(snap);
        var team = snap.TimeoutTeamName ?? penalty?.TeamName ?? LiveFootballChoreography.NotificationTeam(snap);
        text.Text = snap.TimeoutTeamName is not null && period is null
            ? $"TIMEOUT\n{snap.TimeoutTeamName}\n{(team == game.HomeTeamName ? snap.HomeTimeoutsRemaining : snap.AwayTimeoutsRemaining)} remaining"
            : period is null ? penalty?.Banner ?? $"{headline}\n{team}" : $"{headline}\n{game.AwayTeamName} {session.AwayScore} - {session.HomeScore} {game.HomeTeamName}";
        var color = teamColor(team);
        highlight.BackgroundColor = color;
        text.TextColor = color.Red * .2126 + color.Green * .7152 + color.Blue * .0722 > .55 ? Colors.Black : Colors.White;
        highlight.Opacity = 0;
        highlight.Scale = .72;
        highlight.IsVisible = true;
        var completion = new TaskCompletionSource();
        new Animation(progress =>
        {
            highlight.Opacity = progress < .25 ? progress / .25 : progress > .82 ? (1 - progress) / .18 : 1;
            highlight.Scale = .72 + Math.Min(1, progress / .30) * .28 + Math.Sin(progress * Math.PI * 2) * .025;
        }).Commit(highlight, "game-highlight", 16, penalty is not null && period is null ? 2100u : 1250u, Easing.CubicOut,
            (_, _) => completion.TrySetResult());
        await completion.Task;
        highlight.IsVisible = false;
    }

    public static Border Create(out Label text)
    {
        text = new Label { FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center };
        return new Border
        {
            IsVisible = false, StrokeThickness = 2, Stroke = Colors.White, Padding = new Thickness(22, 12),
            HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Content = text
        };
    }
}

public static class GameCastFieldLayout
{
    /// <summary>Keeps portrait fields near the 1.76:1 playing-surface ratio; landscape fills available width.</summary>
    public static void Apply(Border fieldFrame, double width, double height, bool landscape, double portraitHeightShare = .55)
    {
        if (landscape)
        {
            fieldFrame.HeightRequest = -1; fieldFrame.WidthRequest = -1;
            fieldFrame.HorizontalOptions = LayoutOptions.Fill;
            return;
        }
        var ratioHeight = Math.Max(140, Math.Min(Math.Max(0, width - 12) / 1.76, height * portraitHeightShare));
        fieldFrame.HeightRequest = ratioHeight;
        fieldFrame.WidthRequest = ratioHeight * 1.76;
        fieldFrame.HorizontalOptions = LayoutOptions.Center;
    }
}
