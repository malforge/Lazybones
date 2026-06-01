using Lazybones.Localization;

namespace Lazybones.Features.Shell;

// Localized labels for the main disk window, grouped for compiled bindings —
// {Binding Strings.TooltipPlayPause}. See DashboardStrings for the why.
public sealed class MainWindowStrings
{
    private static LocalizationService L => LocalizationService.Instance;

    // Tooltips
    public string TooltipStreak => L.Get("Tooltip_Streak");
    public string TooltipUpdate => L.Get("Tooltip_Update");
    public string TooltipPlayPause => L.Get("Tooltip_PlayPause");
    public string TooltipReset => L.Get("Tooltip_Reset");
    public string TooltipSwap => L.Get("Tooltip_Swap");
    public string TooltipDashboard => L.Get("Tooltip_Dashboard");
    public string TooltipAchievements => L.Get("Tooltip_Achievements");

    // Confirmation / time-adjust dialog buttons
    public string DialogYes => L.Get("Dialog_Yes");
    public string DialogNo => L.Get("Dialog_No");
    public string DialogApply => L.Get("Dialog_Apply");
    public string DialogCancel => L.Get("Dialog_Cancel");

    // Toasts
    public string ToastWelcomeBack => L.Get("Toast_WelcomeBack");
    public string ToastAchievementHeader => L.Get("Toast_AchievementHeader");

    // Time adjustment dialog
    public string TimeAdjustPlaceholder => L.Get("TimeAdjust_Placeholder");
    public string TimeAdjustExamples => L.Get("TimeAdjust_Examples");
    public string TimeAdjustWarning => L.Get("TimeAdjust_Warning");
}
