using System.Collections.Generic;
using Lazybones.Features.Achievements;

namespace Lazybones.Localization;

// en-US is the canonical source: every key the rest of the app references
// must exist here. nb-NO falls back to this table if a key is missing.
internal static class EnUs
{
    public static readonly IReadOnlyDictionary<string, string> Strings = new Dictionary<string, string>
    {
        // -- Dashboard tabs ------------------------------------------------
        ["Tab_Stats"] = "Stats",
        ["Tab_Achievements"] = "Achievements",
        ["Tab_Settings"] = "Settings",
        ["Tab_Updates"] = "Updates",

        // -- Stats tab -----------------------------------------------------
        ["Stats_Today"] = "Today",
        ["Stats_Streak"] = "Streak",
        ["Stats_StreakUnit"] = "days",
        ["Stats_HeatmapLabel"] = "Last 13 weeks · minutes per day",
        ["Stats_BarsLabel"] = "Last 14 days · cycles per day",
        ["Stats_TodayProgressFormat"] = "{0} / {1} cycles",
        ["Stats_TodayMinutesFormat"] = "{0} min stood",

        // -- Achievements tab ----------------------------------------------
        ["Achievements_UnlockedSummary"] = "{0} of {1} unlocked",

        // -- Settings tab --------------------------------------------------
        ["Settings_Language"] = "Language",
        ["Settings_LanguageAuto"] = "Follow OS language",
        ["Settings_OpenAtLogin"] = "Open at login",
        ["Settings_Recommended"] = "Recommended",
        ["Settings_StandingMinutes"] = "Standing time (minutes)",
        ["Settings_SittingMinutes"] = "Sitting time (minutes)",
        ["Settings_DailyCycles"] = "Daily standing cycles",
        ["Settings_DayRollover"] = "Day rolls over at",
        ["Settings_StartEachDay"] = "Start each day",
        ["Settings_StartSeated"] = "Seated",
        ["Settings_StartStanding"] = "Standing",
        ["Settings_AlwaysOnTop"] = "Always on top",
        ["Settings_StandingPausedWhenAway"] = "Standing is paused when away",
        ["Settings_SeatedPausedWhenAway"] = "Seated is paused when away",
        ["Settings_AutoSaveHint"] = "Changes save automatically.",

        // -- Settings tooltips (hover explanations) ------------------------
        ["Settings_OpenAtLogin_Tip"] = "Launch Get Up, Lazybones! automatically when you sign in, so the reminder is always running in the background.",
        ["Settings_Language_Tip"] = "Language for the app's interface. \"Follow OS language\" matches your system; pick a specific language to override it.",
        ["Settings_StandingMinutes_Tip"] = "How long each standing stretch lasts before you're prompted to sit back down.",
        ["Settings_SittingMinutes_Tip"] = "How long you sit before you're prompted to stand up again.",
        ["Settings_DailyCycles_Tip"] = "How many completed standing cycles count as hitting your goal for the day. This drives the streak counter and the outer ring on the disk.",
        ["Settings_DayRollover_Tip"] = "The time of day your daily stats reset and a fresh day begins. A late-night cycle before this time still counts toward the day you just finished, not the next one.",
        ["Settings_StartEachDay_Tip"] = "Which mode the timer starts in when the day rolls over — seated or standing.",
        ["Settings_AlwaysOnTop_Tip"] = "Keep the disk floating above other windows so it stays visible while you work.",
        ["Settings_StandingPausedWhenAway_Tip"] = "When you lock the screen or step away while standing, pause the timer and pick up where you left off on return. Off: the standing cycle keeps counting while you're gone.",
        ["Settings_SeatedPausedWhenAway_Tip"] = "When you lock the screen or step away while seated, pause the timer. Off: the timer keeps running, so time away from your desk counts toward your next stand reminder.",

        // -- Updates tab ---------------------------------------------------
        ["Updates_RunningVersion"] = "Running version",
        ["Updates_StatusGeneric"] = "Status",
        ["Updates_StatusReady"] = "Update ready: v{0}",
        ["Updates_StatusChecking"] = "Checking for updates",
        ["Updates_StatusFailed"] = "Update check failed",
        ["Updates_WhatsNew"] = "What's new",
        ["Updates_CheckButton"] = "Check for updates",
        ["Updates_RestartButton"] = "Restart now",
        ["Updates_DevBuildText"] = "Updates are only available for installed builds — this is a development build.",
        ["Updates_IdleText"] = "Click \"Check for updates\" to see if a newer version is available.",
        ["Updates_CheckingText"] = "Looking for a newer version on GitHub Releases…",
        ["Updates_UpToDateText"] = "You're running the latest version.",
        ["Updates_ReadyTextFormat"] = "Version {0} has been downloaded. It will install on next launch — click \"Restart now\" to install it immediately.",
        ["Updates_FailedText"] = "Something went wrong while checking for updates.",
        ["Updates_NotesLoading"] = "Release notes are loading…",

        // -- Main window timer modes & overlays ----------------------------
        ["Mode_Initializing"] = "Hang on...",
        ["Mode_Paused"] = "Paused...",
        ["Mode_Locked"] = "Locked...",

        // Confirmation dialogs
        ["Dialog_Swap_Title"] = "Swap",
        ["Dialog_Swap_ToSitting"] = "Swap to sitting?",
        ["Dialog_Swap_ToStanding"] = "Swap to standing?",
        ["Dialog_Reset_Title"] = "Reset",
        ["Dialog_Reset_Message"] = "Reset the timer?",
        ["Dialog_Yes"] = "Yes",
        ["Dialog_No"] = "No",
        ["Dialog_Apply"] = "Apply",
        ["Dialog_Cancel"] = "Cancel",

        // Time adjustment dialog
        ["TimeAdjust_Title"] = "Adjust Time",
        ["TimeAdjust_Placeholder"] = "30 or 5m or +2m",
        ["TimeAdjust_Examples"] = "Examples: 30, 1.5, +5m, -10s, 01:30:00",
        ["TimeAdjust_Warning"] = "This cycle won't count toward achievements or streak.",

        // Toasts
        ["Toast_WelcomeBack"] = "Welcome back",
        ["Toast_AchievementHeader"] = "Achievement",
        ["Toast_ResumedHoursFormat"] = "Resumed after {0}h {1}m away",
        ["Toast_ResumedMinutesFormat"] = "Resumed after {0}m away",
        ["Toast_ResumedShort"] = "Resumed",

        // Tooltips on the main disk
        ["Tooltip_PlayPause"] = "Play/Pause",
        ["Tooltip_Reset"] = "Reset",
        ["Tooltip_Swap"] = "Swap standing/resting",
        ["Tooltip_Dashboard"] = "Stats, achievements & settings",
        ["Tooltip_Streak"] = "Daily standing-goal streak — consecutive days at or above your goal. Click to open Stats.",
        ["Tooltip_Update"] = "A new version is ready — click for release notes and to restart",
        ["Tooltip_Achievements"] = "Achievements progress. Click to open the Achievements tab.",

        // -- Mode-switch dialog --------------------------------------------
        ["ModeSwitch_Header"] = "Time to change position.",
        ["ModeSwitch_StartNow"] = "Start Now",
        ["ModeSwitch_Dismiss"] = "Dismiss",

        // -- Achievements: title + description per id ----------------------
        [$"Achievement_{AchievementCatalog.FirstStandId}_Title"] = "First Stand",
        [$"Achievement_{AchievementCatalog.FirstStandId}_Description"] = "You completed your first standing cycle. Welcome!",
        [$"Achievement_{AchievementCatalog.QuickDrawId}_Title"] = "Quick Draw",
        [$"Achievement_{AchievementCatalog.QuickDrawId}_Description"] = "Responded to a prompt within ten seconds.",
        [$"Achievement_{AchievementCatalog.IronLegsId}_Title"] = "Iron Legs",
        [$"Achievement_{AchievementCatalog.IronLegsId}_Description"] = "Stood through a 30+ minute cycle without bailing.",
        [$"Achievement_{AchievementCatalog.EarlyBirdId}_Title"] = "Early Bird",
        [$"Achievement_{AchievementCatalog.EarlyBirdId}_Description"] = "Finished a standing cycle that started before 09:00.",
        [$"Achievement_{AchievementCatalog.NightOwlId}_Title"] = "Night Owl",
        [$"Achievement_{AchievementCatalog.NightOwlId}_Description"] = "Finished a standing cycle past 22:00.",

        [$"Achievement_{AchievementCatalog.WarmingUpId}_Title"] = "Warming Up",
        [$"Achievement_{AchievementCatalog.WarmingUpId}_Description"] = "Three days in a row at goal. The habit begins.",
        [$"Achievement_{AchievementCatalog.SevenDayStreakId}_Title"] = "7-Day Streak",
        [$"Achievement_{AchievementCatalog.SevenDayStreakId}_Description"] = "Seven days of hitting your daily goal in a row.",
        [$"Achievement_{AchievementCatalog.TwoWeekWonderId}_Title"] = "Two-Week Wonder",
        [$"Achievement_{AchievementCatalog.TwoWeekWonderId}_Description"] = "Fourteen straight days. You're not playing.",
        [$"Achievement_{AchievementCatalog.HabitFormedId}_Title"] = "Habit Formed",
        [$"Achievement_{AchievementCatalog.HabitFormedId}_Description"] = "Thirty days. Whatever this was, it's a habit now.",

        [$"Achievement_{AchievementCatalog.DailyDriverId}_Title"] = "Daily Driver",
        [$"Achievement_{AchievementCatalog.DailyDriverId}_Description"] = "Completed five standing cycles in one day.",
        [$"Achievement_{AchievementCatalog.OverachieverId}_Title"] = "Overachiever",
        [$"Achievement_{AchievementCatalog.OverachieverId}_Description"] = "Stood for 1.5× your daily goal in a single day.",
        [$"Achievement_{AchievementCatalog.DoubleDownId}_Title"] = "Double Down",
        [$"Achievement_{AchievementCatalog.DoubleDownId}_Description"] = "Doubled your daily goal in a single day.",
        [$"Achievement_{AchievementCatalog.PerfectDayId}_Title"] = "Perfect Day",
        [$"Achievement_{AchievementCatalog.PerfectDayId}_Description"] = "Hit your daily goal without dismissing a single prompt.",

        [$"Achievement_{AchievementCatalog.CenturionId}_Title"] = "Centurion",
        [$"Achievement_{AchievementCatalog.CenturionId}_Description"] = "Completed 100 standing cycles.",
        [$"Achievement_{AchievementCatalog.LongHaulId}_Title"] = "Long Haul",
        [$"Achievement_{AchievementCatalog.LongHaulId}_Description"] = "Ten cumulative hours of standing. Your back thanks you.",
        [$"Achievement_{AchievementCatalog.MountaineerId}_Title"] = "Mountaineer",
        [$"Achievement_{AchievementCatalog.MountaineerId}_Description"] = "One hundred cumulative hours of standing.",
    };

    // Random text pools. Pick(poolName) returns one entry at random per call;
    // pools intentionally vary in tone/length so the disk doesn't feel scripted.
    public static readonly IReadOnlyDictionary<string, string[]> TextPools = new Dictionary<string, string[]>
    {
        ["SitDown"] =
        [
            "You can sit now.",
            "Rest your legs.",
            "Sit down and relax.",
            "Sit comfortably.",
            "Take a seat.",
            "Ok, sit.",
            "Time to sit down.",
            "Get your chair.",
            "You can take a break now.",
            "Butt on the chair.",
        ],

        ["StandUp"] =
        [
            "Stand up now!",
            "Up, up, up!",
            "Raise your desk!",
            "Get up, get up!",
            "Stand tall!",
            "Time to stand up!",
            "Shift to standing position!",
            "Up, soldier!",
            "Up, lazybones!",
            "Get your a** up!",
        ],

        ["NewDay"] =
        [
            "New day!",
            "Fresh start.",
            "Good morning!",
            "Rise and shine.",
            "Another lap around the sun.",
            "Day one. Again.",
            "Clean slate.",
            "Here we go again.",
            "Day reset.",
            "Onwards!",
        ],
    };
}
