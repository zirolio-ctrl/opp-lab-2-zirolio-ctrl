using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using opp_lab_2_zirolio_ctrl.Services;
using opp_lab_2_zirolio_ctrl.ViewModels;
using opp_lab_2_zirolio_ctrl.Views;

namespace opp_lab_2_zirolio_ctrl;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(new DialogService())
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}