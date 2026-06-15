using System;
using Lazybones.Core.State;

namespace Lazybones.Features.StartAtLogin;

public interface IStartupService
{
    bool IsEnabled { get; }

    /// <summary>
    /// Attempts to enable or disable launch-at-login. Returns true on success, false if the
    /// underlying OS write failed (e.g. registry access denied, sandboxed filesystem).
    /// On false, the caller should treat the persisted state as "not enabled" — the existing
    /// callers (DashboardViewModel.StartWithWindows setter, MainWindowViewModel.PromptStartWithWindows)
    /// use the (SetEnabled(value) && value) pattern to enforce this.
    /// </summary>
    bool SetEnabled(bool enabled);
}

public static class StartupService
{
    public static IStartupService Instance { get; } = Create();

    /// <summary>
    /// Re-asserts the user's saved launch-at-login preference onto the OS at
    /// startup. The persisted <see cref="AppState.StartWithWindows"/> flag is the
    /// source of intent, but the actual Run-key / LaunchAgent entry can drift out
    /// from under it: the entry was written under the app's old name/bundle id
    /// (StandUp; com.malforge.*), an "on" preference rode across the rename data
    /// migration without a matching entry on this machine, the install moved to a
    /// new path, or an external cleanup removed it. Because the dashboard checkbox
    /// reflects the stored flag — not the live OS state — such drift silently
    /// leaves the toggle "on" while nothing launches at login.
    ///
    /// When intent is "on" we re-assert unconditionally rather than only when the
    /// OS reports "off": on macOS a leftover legacy plist makes IsEnabled report
    /// "on" while still pointing at the pre-rename executable, so guarding on
    /// IsEnabled would skip a stale-but-present entry and leave autostart broken.
    /// SetEnabled(true) is idempotent on both platforms — it rewrites the current
    /// entry with the current executable path and clears any legacy entries — so
    /// calling it on every launch keeps intent and OS in sync at negligible cost.
    /// The result is intentionally not written back to state: a transient OS-write
    /// failure shouldn't erase the user's preference; the next launch retries.
    /// </summary>
    public static void Reconcile(AppState state)
    {
        if (state.StartWithWindows)
            Instance.SetEnabled(true);
    }

    private static IStartupService Create()
    {
        if (OperatingSystem.IsWindows())
        {
#if WINDOWS
            return new WindowsStartupService();
#endif
        }

        if (OperatingSystem.IsMacOS())
            return new MacStartupService();

        return new NoOpStartupService();
    }
}

internal sealed class NoOpStartupService : IStartupService
{
    public bool IsEnabled => false;
    public bool SetEnabled(bool enabled) => false;
}
