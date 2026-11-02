using DynastyManager.Core.Models;
using Microsoft.Maui.Controls;

namespace Football.Presentation;

public static class PhaseTaskComponents
{
    // Hosts retain styles, callbacks and authority guards; only layout/task data is shared.
    public static Grid Grid(IEnumerable<PhaseTask> tasks, Func<PhaseTask, View> render, double spacing = 8)
    {
        var grid = new Grid { ColumnSpacing = spacing, RowSpacing = spacing,
            ColumnDefinitions = new() { new(GridLength.Star), new(GridLength.Star) } };
        var items = tasks.ToArray();
        for (var row = 0; row < (items.Length + 1) / 2; row++) grid.RowDefinitions.Add(new(GridLength.Auto));
        for (var i = 0; i < items.Length; i++)
        {
            var view = render(items[i]); view.AutomationId = items[i].Id;
            grid.Add(view, i % 2, i / 2);
        }
        return grid;
    }
}
