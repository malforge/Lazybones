using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lazybones.Core.State;
using Lazybones.Features.Shell;
using Lazybones.Localization;

namespace Lazybones;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Apply the saved language preference before any window loads so the
        // first paint already speaks the chosen language; empty preference
        // resolves to OS UI culture.
        LocalizationService.Instance.Apply(AppState.LoadState().Language);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}