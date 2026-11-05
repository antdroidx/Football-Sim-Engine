using DynastyManager.Core.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace DynastyManager.App;

public static class FootballNewsStoryCard
{
    public static Border Create(FootballNewsStory story, View? icon, Color accent)
    {
        ArgumentNullException.ThrowIfNull(story);
        var badge = icon ?? new Border
        {
            WidthRequest = 44,
            HeightRequest = 44,
            Padding = 0,
            StrokeThickness = 0,
            BackgroundColor = accent,
            Content = new Label
            {
                Text = string.IsNullOrWhiteSpace(story.Category) ? "•" : story.Category[..1].ToUpperInvariant(),
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var media = story.IsSocial ? "IN-GAME MEDIA" : story.Category.ToUpperInvariant();
        var metadata = new Label
        {
            Text = $"{media} · {story.SeasonYear} · Week {story.Week}",
            TextColor = accent,
            FontSize = 10,
            LineBreakMode = LineBreakMode.TailTruncation,
            HorizontalOptions = LayoutOptions.Start
        };
        var headline = new Label
        {
            Text = story.Headline,
            TextColor = DynastyTheme.Text,
            FontAttributes = FontAttributes.Bold,
            FontSize = 17,
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalOptions = LayoutOptions.Start
        };
        var preview = new Label
        {
            Text = story.Body,
            TextColor = DynastyTheme.Muted,
            FontSize = 12,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 3
        };
        var read = new Label
        {
            Text = story.IsSocial ? "Open in-game media ›" : "Read story ›",
            TextColor = accent,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Start
        };

        var heading = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 4,
            HorizontalOptions = LayoutOptions.Fill,
            ColumnDefinitions = new ColumnDefinitionCollection { new(44), new(GridLength.Star) }
        };
        heading.Add(badge, 0, 0);
        heading.Add(new VerticalStackLayout { Spacing = 4, Children = { metadata, headline } }, 1, 0);
        heading.VerticalOptions = LayoutOptions.Start;

        var layout = new VerticalStackLayout { Spacing = 10, Children = { heading, preview, read } };
        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(4), new(GridLength.Star) }
        };
        var accentBar = new BoxView
        {
            Color = accent,
            CornerRadius = 4,
            WidthRequest = 4,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill
        };
        content.Add(accentBar, 0, 0);
        content.Add(new VerticalStackLayout { Spacing = 10, Padding = new Thickness(12, 4, 4, 4), Children = { layout } }, 1, 0);

        return new Border
        {
            AutomationId = "news-story-card",
            StrokeThickness = 0,
            Padding = 10,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = DynastyTheme.Surface,
            Content = content
        };
    }
}
