using Microsoft.Maui.Controls.Shapes;

namespace DynastyManager.App;

/// <summary>
/// Shared presentation for the first-run setup screens in both games. The opening screens read as
/// a short, labelled form: a title, a few grouped cards, and one clear action at the end.
/// </summary>
public static class SetupScreen
{
    public static Color Ink => DynastyTheme.Text;
    public static Color InkMuted => DynastyTheme.Muted;

    /// <summary>Large title with a one-line explanation of the screen.</summary>
    public static View Hero(string title, string subtitle) =>
        new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label { Text = title, FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Ink },
                new Label { Text = subtitle, FontSize = 13, TextColor = InkMuted }
            }
        };

    /// <summary>A grouped block of related choices.</summary>
    public static View Card(string title, string? subtitle, params View[] content)
    {
        var stack = new VerticalStackLayout { Spacing = 10 };
        stack.Add(new Label { Text = title, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Ink });
        if (!string.IsNullOrWhiteSpace(subtitle))
            stack.Add(new Label { Text = subtitle, FontSize = 12, TextColor = InkMuted });
        foreach (var view in content) stack.Add(view);
        return new Border
        {
            BackgroundColor = DynastyTheme.Surface,
            Stroke = DynastyTheme.ControlEdge,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(16),
            Content = stack
        };
    }

    /// <summary>A labelled control with an optional hint under it.</summary>
    public static View Field(string label, string? hint, View control)
    {
        var stack = new VerticalStackLayout { Spacing = 4 };
        stack.Add(new Label { Text = label, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = InkMuted });
        if (!string.IsNullOrWhiteSpace(hint))
            stack.Add(new Label { Text = hint, FontSize = 11, TextColor = InkMuted });
        stack.Add(control);
        return stack;
    }

    /// <summary>A labelled switch that reads as a single row.</summary>
    public static View Toggle(string title, string subtitle, Switch toggle)
    {
        var row = new Grid
        {
            ColumnDefinitions = new() { new(GridLength.Star), new(GridLength.Auto) },
            ColumnSpacing = 12
        };
        var copy = new VerticalStackLayout { Spacing = 2 };
        copy.Add(new Label { Text = title, FontSize = 14, TextColor = Ink });
        copy.Add(new Label { Text = subtitle, FontSize = 12, TextColor = InkMuted });
        row.Add(copy, 0, 0);
        row.Add(toggle, 1, 0);
        toggle.VerticalOptions = LayoutOptions.Center;
        var border = new Border
        {
            BackgroundColor = DynastyTheme.Panel,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(14),
            Content = row
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => toggle.IsToggled = !toggle.IsToggled;
        border.GestureRecognizers.Add(tap);
        return border;
    }

    /// <summary>The single action that completes the screen.</summary>
    public static Button Primary(string text, Action action, string? automationId = null)
    {
        var button = new Button
        {
            Text = text,
            AutomationId = automationId,
            BackgroundColor = DynastyTheme.Accent,
            TextColor = DynastyTheme.Background,
            FontAttributes = FontAttributes.Bold,
            FontSize = 16,
            CornerRadius = 12,
            HeightRequest = 52,
            Padding = new Thickness(16, 0)
        };
        button.Clicked += (_, _) => action();
        return button;
    }

    /// <summary>A quiet alternative to the primary action.</summary>
    public static Button Secondary(string text, Action action, string? automationId = null)
    {
        var button = new Button
        {
            Text = text,
            AutomationId = automationId,
            BackgroundColor = Colors.Transparent,
            TextColor = InkMuted,
            FontSize = 15,
            BorderColor = DynastyTheme.ControlEdge,
            BorderWidth = 1,
            CornerRadius = 12,
            HeightRequest = 48
        };
        button.Clicked += (_, _) => action();
        return button;
    }

    /// <summary>A centred column that keeps a form readable on a wide screen.</summary>
    public static View Column(params View[] children)
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 14,
            MaximumWidthRequest = 560,
            HorizontalOptions = LayoutOptions.Center
        };
        foreach (var child in children) stack.Add(child);
        return stack;
    }

    /// <summary>The closing block of a setup screen: the action, then its quiet alternative.</summary>
    public static View Footer(View primary, View secondary) =>
        new VerticalStackLayout
        {
            Spacing = 8,
            Children = { primary, secondary }
        };
}
