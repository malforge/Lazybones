using Lazybones.Localization;

namespace Lazybones.Features.Shell;

// Localized labels for the mode-switch dialog, grouped for compiled bindings.
// The dialog is transient (no live language switch during its lifetime), so
// the owning view model doesn't re-raise these — they resolve once at bind
// time against the active culture. See DashboardStrings for the why.
public sealed class ModeSwitchStrings
{
    private static LocalizationService L => LocalizationService.Instance;

    public string ModeSwitchHeader => L.Get("ModeSwitch_Header");
    public string ModeSwitchStartNow => L.Get("ModeSwitch_StartNow");
    public string ModeSwitchDismiss => L.Get("ModeSwitch_Dismiss");
}
