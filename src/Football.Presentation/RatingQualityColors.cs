using Microsoft.Maui.Graphics;

namespace Football.Presentation;

public static class RatingQualityColors
{
    public static Color ForRating(int rating, bool lightTheme = false)
    {
        rating = Math.Clamp(rating, 0, 99);
        return lightTheme
            ? rating switch
            {
                >= 90 => Color.FromArgb("#6941C6"),
                >= 85 => Color.FromArgb("#1D5FA7"),
                >= 80 => Color.FromArgb("#087F6A"),
                >= 70 => Color.FromArgb("#237A3B"),
                >= 60 => Color.FromArgb("#8A6D00"),
                >= 50 => Color.FromArgb("#B54708"),
                _ => Color.FromArgb("#B42318")
            }
            : rating switch
            {
                >= 90 => Color.FromArgb("#B69CFF"),
                >= 85 => Color.FromArgb("#74B9FF"),
                >= 80 => Color.FromArgb("#57D1A8"),
                >= 70 => Color.FromArgb("#7BD389"),
                >= 60 => Color.FromArgb("#E4C75A"),
                >= 50 => Color.FromArgb("#FF9E64"),
                _ => Color.FromArgb("#FF7B72")
            };
    }
}
