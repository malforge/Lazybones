using Lazybones.Localization;

namespace Lazybones.Features.Dashboard;

// Localized labels for the dashboard view, grouped so XAML can bind them with
// compiled bindings — {Binding Strings.TabStats} — instead of a reflection
// markup extension (which the full-trim Release build strips; see the removed
// LocalizeExtension). Values are read live from LocalizationService. On a
// language switch the owning view model swaps in a fresh instance and raises
// PropertyChanged(nameof(Strings)); the reference change is what makes compiled
// bindings re-read every leaf, since these objects don't raise per-property
// INPC. A same-reference notify alone would not refresh the labels.
public sealed class DashboardStrings
{
    private static LocalizationService L => LocalizationService.Instance;

    // Tabs
    public string TabStats => L.Get("Tab_Stats");
    public string TabAchievements => L.Get("Tab_Achievements");
    public string TabSettings => L.Get("Tab_Settings");
    public string TabUpdates => L.Get("Tab_Updates");

    // Stats tab
    public string StatsToday => L.Get("Stats_Today");
    public string StatsStreak => L.Get("Stats_Streak");
    public string StatsStreakUnit => L.Get("Stats_StreakUnit");
    public string StatsHeatmapLabel => L.Get("Stats_HeatmapLabel");
    public string StatsBarsLabel => L.Get("Stats_BarsLabel");

    // Settings tab
    public string SettingsOpenAtLogin => L.Get("Settings_OpenAtLogin");
    public string SettingsRecommended => L.Get("Settings_Recommended");
    public string SettingsLanguage => L.Get("Settings_Language");
    public string SettingsStandingMinutes => L.Get("Settings_StandingMinutes");
    public string SettingsSittingMinutes => L.Get("Settings_SittingMinutes");
    public string SettingsDailyCycles => L.Get("Settings_DailyCycles");
    public string SettingsDayRollover => L.Get("Settings_DayRollover");
    public string SettingsStartEachDay => L.Get("Settings_StartEachDay");
    public string SettingsStartSeated => L.Get("Settings_StartSeated");
    public string SettingsStartStanding => L.Get("Settings_StartStanding");
    public string SettingsAlwaysOnTop => L.Get("Settings_AlwaysOnTop");
    public string SettingsStandingPausedWhenAway => L.Get("Settings_StandingPausedWhenAway");
    public string SettingsSeatedPausedWhenAway => L.Get("Settings_SeatedPausedWhenAway");
    public string SettingsAutoSaveHint => L.Get("Settings_AutoSaveHint");

    // Setting tooltips — plain-language explanations shown on hover.
    public string SettingsOpenAtLoginTip => L.Get("Settings_OpenAtLogin_Tip");
    public string SettingsLanguageTip => L.Get("Settings_Language_Tip");
    public string SettingsStandingMinutesTip => L.Get("Settings_StandingMinutes_Tip");
    public string SettingsSittingMinutesTip => L.Get("Settings_SittingMinutes_Tip");
    public string SettingsDailyCyclesTip => L.Get("Settings_DailyCycles_Tip");
    public string SettingsDayRolloverTip => L.Get("Settings_DayRollover_Tip");
    public string SettingsStartEachDayTip => L.Get("Settings_StartEachDay_Tip");
    public string SettingsAlwaysOnTopTip => L.Get("Settings_AlwaysOnTop_Tip");
    public string SettingsStandingPausedWhenAwayTip => L.Get("Settings_StandingPausedWhenAway_Tip");
    public string SettingsSeatedPausedWhenAwayTip => L.Get("Settings_SeatedPausedWhenAway_Tip");

    // Updates tab
    public string UpdatesRunningVersion => L.Get("Updates_RunningVersion");
    public string UpdatesWhatsNew => L.Get("Updates_WhatsNew");
    public string UpdatesCheckButton => L.Get("Updates_CheckButton");
    public string UpdatesRestartButton => L.Get("Updates_RestartButton");
}
