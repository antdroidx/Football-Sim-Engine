using Microsoft.Maui.Controls;

namespace Football.Presentation;

public static class StaffDirectoryComponents
{
    // League hosts own navigation/permissions and retain their link styling.
    // A stable identity also makes duplicate names and table reordering testable.
    public static View Name(Guid id, string name, Func<string, Func<Task>, View> link, Func<Guid, Task> open)
    {
        var view = link(name, () => open(id));
        view.AutomationId = "staff-profile-" + id.ToString("N");
        SemanticProperties.SetDescription(view, "View staff profile " + name);
        return view;
    }
}
