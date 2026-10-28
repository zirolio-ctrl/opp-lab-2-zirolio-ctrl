using Avalonia.Controls;
using opp_lab_2_zirolio_ctrl.ViewModels;

namespace opp_lab_2_zirolio_ctrl.Views;

public partial class MainWindow : Window
{
    private bool _canClose;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (_canClose) return;

        e.Cancel = true;
        if (DataContext is MainViewModel vm && await vm.ConfirmExitAsync())
        {
            _canClose = true;
            Close();
        }
    }
}