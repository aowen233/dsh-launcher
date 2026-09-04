namespace DshLauncher;

internal static class DialogService
{
    private const string Caption = "DSH Launcher";

    public static bool Confirm(string heading, string text, bool warning = false)
    {
        var page = new TaskDialogPage
        {
            Caption = Caption,
            Heading = heading,
            Text = text,
            Icon = warning ? TaskDialogIcon.Warning : TaskDialogIcon.Information,
            AllowCancel = true,
        };
        page.Buttons.Add(TaskDialogButton.Yes);
        page.Buttons.Add(TaskDialogButton.No);
        return TaskDialog.ShowDialog(page) == TaskDialogButton.Yes;
    }

    public static void Information(string heading, string text)
    {
        Show(heading, text, TaskDialogIcon.Information);
    }

    public static void Error(string heading, string text)
    {
        Show(heading, text, TaskDialogIcon.Error);
    }

    private static void Show(string heading, string text, TaskDialogIcon icon)
    {
        var page = new TaskDialogPage
        {
            Caption = Caption,
            Heading = heading,
            Text = text,
            Icon = icon,
            AllowCancel = true,
        };
        page.Buttons.Add(TaskDialogButton.OK);
        TaskDialog.ShowDialog(page);
    }
}
