using Microsoft.Maui.Controls;

namespace Football.Presentation;

public static class StaffProfileComponents
{
    public static View RatingBar(string label, int rating, string? automationId = null) =>
        new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label { Text = $"{label} {rating}", FontSize = 14, FontAttributes = FontAttributes.Bold },
                new ProgressBar
                {
                    Progress = Math.Clamp(rating / 99d, 0, 1),
                    ProgressColor = RatingQualityColors.ForRating(rating),
                    HeightRequest = 8,
                    AutomationId = automationId ?? ""
                }
            }
        };
}
