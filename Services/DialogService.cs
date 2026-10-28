using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Platform.Storage;

namespace opp_lab_2_zirolio_ctrl.Services;

public sealed class DialogService : IDialogService
{
    private static Window? Main =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    public async Task<string?> PickXmlFileAsync()
    {
        if (Main is null) return null;
        var files = await Main.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Оберіть XML",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("XML") { Patterns = new[] { "*.xml" } } }
        });
        return files.FirstOrDefault()?.Path.LocalPath;
    }

    public async Task<string?> PickSaveFileAsync(string suggestedName, string ext)
    {
        if (Main is null) return null;
        var file = await Main.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Зберегти файл",
            SuggestedFileName = suggestedName,
            DefaultExtension = ext,
            FileTypeChoices = new[]
            {
                new FilePickerFileType(ext.ToUpper()) { Patterns = new[] { $"*.{ext}" } }
            }
        });
        return file?.Path.LocalPath;
    }

    public async Task<bool> ConfirmAsync(string title, string message)
    {
        if (Main is null) return false;

        var yes = new Button { Content = "Так", MinWidth = 80 };
        var no = new Button { Content = "Ні", MinWidth = 80 };

        var dialog = new Window
        {
            Title = title,
            Width = 380,
            Height = 150,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 20,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 10,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Children = { yes, no }
                    }
                }
            }
        };

        yes.Click += (_, _) => dialog.Close(true);
        no.Click += (_, _) => dialog.Close(false);

        return await dialog.ShowDialog<bool>(Main);
    }

    public void CloseMainWindow() => Main?.Close();

    public void OpenFile(string path) => Process.Start(new ProcessStartInfo
    {
        FileName = path,
        UseShellExecute = true
    });
}