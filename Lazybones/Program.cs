using Avalonia;
using System;
using Lazybones.Core.State;
using Lazybones.Features.StartAtLogin;
using Velopack;

namespace Lazybones;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Must run before Avalonia: handles the --veloapp-* hooks the Velopack
        // installer/updater invokes (first-install, uninstall, post-update, etc.)
        // and exits the process for those modes. No-op on normal launches.
        var velopack = VelopackApp.Build();
        // Remove the launch-at-login entry on uninstall so we don't leave a
        // dangling Run-key value pointing at a deleted exe. Velopack's uninstall
        // fast-callback is Windows-only (Update.exe re-invokes the app in the
        // user's context, so HKCU is the right scope); macOS uninstall fires no
        // equivalent hook, so this cleanup is necessarily Windows-scoped.
        if (OperatingSystem.IsWindows())
            velopack = velopack.OnBeforeUninstallFastCallback(_ => StartupService.Instance.SetEnabled(false));
        velopack.Run();

        // Re-assert launch-at-login against the OS before the UI loads, so a
        // saved "on" preference whose registry entry drifted away (app rename,
        // data migration, external cleanup) actually starts working again
        // instead of the checkbox silently lying about it.
        StartupService.Reconcile(AppState.LoadState());

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
