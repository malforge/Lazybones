using System.Collections.Generic;
using Lazybones.Features.Achievements;

namespace Lazybones.Localization;

// Norwegian Bokmål translations. Missing keys fall back to EnUs.Strings —
// new strings can be added to EnUs first and translated here later without
// breaking the build.
internal static class NbNo
{
    public static readonly IReadOnlyDictionary<string, string> Strings = new Dictionary<string, string>
    {
        // -- Dashboard tabs ------------------------------------------------
        ["Tab_Stats"] = "Statistikk",
        ["Tab_Achievements"] = "Prestasjoner",
        ["Tab_Settings"] = "Innstillinger",
        ["Tab_Updates"] = "Oppdateringer",

        // -- Stats tab -----------------------------------------------------
        ["Stats_Today"] = "I dag",
        ["Stats_Streak"] = "Rekke",
        ["Stats_StreakUnit"] = "dager",
        ["Stats_HeatmapLabel"] = "Siste 13 uker · minutter per dag",
        ["Stats_BarsLabel"] = "Siste 14 dager · sykluser per dag",
        ["Stats_TodayProgressFormat"] = "{0} / {1} sykluser",
        ["Stats_TodayMinutesFormat"] = "{0} min stått",

        // -- Achievements tab ----------------------------------------------
        ["Achievements_UnlockedSummary"] = "{0} av {1} låst opp",

        // -- Settings tab --------------------------------------------------
        ["Settings_Language"] = "Språk",
        ["Settings_LanguageAuto"] = "Følg systemspråk",
        ["Settings_OpenAtLogin"] = "Åpne ved pålogging",
        ["Settings_Recommended"] = "Anbefalt",
        ["Settings_StandingMinutes"] = "Ståtid (minutter)",
        ["Settings_SittingMinutes"] = "Sittetid (minutter)",
        ["Settings_DailyCycles"] = "Stå-sykluser per dag",
        ["Settings_DayRollover"] = "Dagen starter kl.",
        ["Settings_StartEachDay"] = "Start hver dag",
        ["Settings_StartSeated"] = "Sittende",
        ["Settings_StartStanding"] = "Stående",
        ["Settings_AlwaysOnTop"] = "Alltid øverst",
        ["Settings_StandingPausedWhenAway"] = "Stående settes på pause når du er borte",
        ["Settings_SeatedPausedWhenAway"] = "Sittende settes på pause når du er borte",
        ["Settings_AutoSaveHint"] = "Endringer lagres automatisk.",

        // -- Settings tooltips (hover-forklaringer) ------------------------
        ["Settings_OpenAtLogin_Tip"] = "Start Get Up, Lazybones! automatisk når du logger inn, slik at påminneren alltid kjører i bakgrunnen.",
        ["Settings_Language_Tip"] = "Språk for appens grensesnitt. \"Følg systemspråk\" matcher systemet ditt; velg et bestemt språk for å overstyre det.",
        ["Settings_StandingMinutes_Tip"] = "Hvor lenge hver ståøkt varer før du blir bedt om å sette deg igjen.",
        ["Settings_SittingMinutes_Tip"] = "Hvor lenge du sitter før du blir bedt om å reise deg igjen.",
        ["Settings_DailyCycles_Tip"] = "Hvor mange fullførte stå-sykluser som teller som å nå dagsmålet. Dette styrer rekketelleren og den ytre ringen på skiven.",
        ["Settings_DayRollover_Tip"] = "Tidspunktet på dagen da dagsstatistikken nullstilles og en ny dag begynner. En sen syklus før dette tidspunktet teller fortsatt mot dagen du nettopp avsluttet, ikke den neste.",
        ["Settings_StartEachDay_Tip"] = "Hvilken modus tidtakeren starter i når dagen ruller over — sittende eller stående.",
        ["Settings_AlwaysOnTop_Tip"] = "Hold skiven flytende over andre vinduer slik at den er synlig mens du jobber.",
        ["Settings_StandingPausedWhenAway_Tip"] = "Når du låser skjermen eller går fra mens du står, settes tidtakeren på pause og fortsetter der du slapp når du kommer tilbake. Av: stå-syklusen fortsetter å telle mens du er borte.",
        ["Settings_SeatedPausedWhenAway_Tip"] = "Når du låser skjermen eller går fra mens du sitter, settes tidtakeren på pause. Av: tidtakeren fortsetter å gå, så tid borte fra pulten teller mot neste påminnelse om å reise deg.",

        // -- Updates tab ---------------------------------------------------
        ["Updates_RunningVersion"] = "Kjørende versjon",
        ["Updates_StatusGeneric"] = "Status",
        ["Updates_StatusReady"] = "Oppdatering klar: v{0}",
        ["Updates_StatusChecking"] = "Ser etter oppdateringer",
        ["Updates_StatusFailed"] = "Oppdateringssjekk mislyktes",
        ["Updates_WhatsNew"] = "Hva er nytt",
        ["Updates_CheckButton"] = "Se etter oppdateringer",
        ["Updates_RestartButton"] = "Start på nytt nå",
        ["Updates_DevBuildText"] = "Oppdateringer er bare tilgjengelige for installerte bygg — dette er et utviklerbygg.",
        ["Updates_IdleText"] = "Klikk \"Se etter oppdateringer\" for å sjekke om en nyere versjon er tilgjengelig.",
        ["Updates_CheckingText"] = "Ser etter en nyere versjon på GitHub Releases…",
        ["Updates_UpToDateText"] = "Du kjører den nyeste versjonen.",
        ["Updates_ReadyTextFormat"] = "Versjon {0} er lastet ned. Den installeres ved neste oppstart — klikk \"Start på nytt nå\" for å installere den med en gang.",
        ["Updates_FailedText"] = "Noe gikk galt under oppdateringssjekken.",
        ["Updates_NotesLoading"] = "Versjonsnotater lastes inn…",

        // -- Main window timer modes & overlays ----------------------------
        ["Mode_Initializing"] = "Vent litt...",
        ["Mode_Paused"] = "Pause...",
        ["Mode_Locked"] = "Låst...",

        // Confirmation dialogs
        ["Dialog_Swap_Title"] = "Bytt",
        ["Dialog_Swap_ToSitting"] = "Bytte til sittende?",
        ["Dialog_Swap_ToStanding"] = "Bytte til stående?",
        ["Dialog_Reset_Title"] = "Tilbakestill",
        ["Dialog_Reset_Message"] = "Tilbakestille tidtakeren?",
        ["Dialog_Yes"] = "Ja",
        ["Dialog_No"] = "Nei",
        ["Dialog_Apply"] = "Bruk",
        ["Dialog_Cancel"] = "Avbryt",

        // Time adjustment dialog
        ["TimeAdjust_Title"] = "Juster tid",
        ["TimeAdjust_Placeholder"] = "30 eller 5m eller +2m",
        ["TimeAdjust_Examples"] = "Eksempler: 30, 1,5, +5m, -10s, 01:30:00",
        ["TimeAdjust_Warning"] = "Denne syklusen teller ikke mot prestasjoner eller rekke.",

        // Toasts
        ["Toast_WelcomeBack"] = "Velkommen tilbake",
        ["Toast_AchievementHeader"] = "Prestasjon",
        ["Toast_ResumedHoursFormat"] = "Fortsatte etter {0}t {1}m borte",
        ["Toast_ResumedMinutesFormat"] = "Fortsatte etter {0}m borte",
        ["Toast_ResumedShort"] = "Fortsatte",

        // Tooltips on the main disk
        ["Tooltip_PlayPause"] = "Spill/pause",
        ["Tooltip_Reset"] = "Tilbakestill",
        ["Tooltip_Swap"] = "Bytt stående/sittende",
        ["Tooltip_Dashboard"] = "Statistikk, prestasjoner og innstillinger",
        ["Tooltip_Streak"] = "Daglig stå-mål-rekke — sammenhengende dager med eller over målet ditt. Klikk for å åpne Statistikk.",
        ["Tooltip_Update"] = "En ny versjon er klar — klikk for versjonsnotater og omstart",
        ["Tooltip_Achievements"] = "Prestasjonsfremdrift. Klikk for å åpne Prestasjoner-fanen.",

        // -- Mode-switch dialog --------------------------------------------
        ["ModeSwitch_Header"] = "På tide å bytte stilling.",
        ["ModeSwitch_StartNow"] = "Start nå",
        ["ModeSwitch_Dismiss"] = "Avvis",

        // -- Achievements: title + description per id ----------------------
        [$"Achievement_{AchievementCatalog.FirstStandId}_Title"] = "Først ute",
        [$"Achievement_{AchievementCatalog.FirstStandId}_Description"] = "Du fullførte din første stå-syklus. Velkommen!",
        [$"Achievement_{AchievementCatalog.QuickDrawId}_Title"] = "Lynrask",
        [$"Achievement_{AchievementCatalog.QuickDrawId}_Description"] = "Svarte på en påminnelse innen ti sekunder.",
        [$"Achievement_{AchievementCatalog.IronLegsId}_Title"] = "Jernbein",
        [$"Achievement_{AchievementCatalog.IronLegsId}_Description"] = "Sto gjennom en 30+ minutters syklus uten å gi opp.",
        [$"Achievement_{AchievementCatalog.EarlyBirdId}_Title"] = "Morgenfugl",
        [$"Achievement_{AchievementCatalog.EarlyBirdId}_Description"] = "Fullførte en stå-syklus som startet før 09:00.",
        [$"Achievement_{AchievementCatalog.NightOwlId}_Title"] = "Nattugle",
        [$"Achievement_{AchievementCatalog.NightOwlId}_Description"] = "Fullførte en stå-syklus etter 22:00.",

        [$"Achievement_{AchievementCatalog.WarmingUpId}_Title"] = "Varmer opp",
        [$"Achievement_{AchievementCatalog.WarmingUpId}_Description"] = "Tre dager på rad på mål. Vanen begynner.",
        [$"Achievement_{AchievementCatalog.SevenDayStreakId}_Title"] = "7-dagers rekke",
        [$"Achievement_{AchievementCatalog.SevenDayStreakId}_Description"] = "Syv dager på rad med daglig mål nådd.",
        [$"Achievement_{AchievementCatalog.TwoWeekWonderId}_Title"] = "To-ukers-vidunderet",
        [$"Achievement_{AchievementCatalog.TwoWeekWonderId}_Description"] = "Fjorten dager på rad. Du tuller ikke.",
        [$"Achievement_{AchievementCatalog.HabitFormedId}_Title"] = "Vanen er dannet",
        [$"Achievement_{AchievementCatalog.HabitFormedId}_Description"] = "Tretti dager. Hva enn dette var, er det en vane nå.",

        [$"Achievement_{AchievementCatalog.DailyDriverId}_Title"] = "Hverdagshelt",
        [$"Achievement_{AchievementCatalog.DailyDriverId}_Description"] = "Fullførte fem stå-sykluser på én dag.",
        [$"Achievement_{AchievementCatalog.OverachieverId}_Title"] = "Overpresterer",
        [$"Achievement_{AchievementCatalog.OverachieverId}_Description"] = "Sto for 1,5× ditt daglige mål på én dag.",
        [$"Achievement_{AchievementCatalog.DoubleDownId}_Title"] = "Dobbelt opp",
        [$"Achievement_{AchievementCatalog.DoubleDownId}_Description"] = "Doblet ditt daglige mål på én dag.",
        [$"Achievement_{AchievementCatalog.PerfectDayId}_Title"] = "Perfekt dag",
        [$"Achievement_{AchievementCatalog.PerfectDayId}_Description"] = "Nådde det daglige målet uten å avvise en eneste påminnelse.",

        [$"Achievement_{AchievementCatalog.CenturionId}_Title"] = "Hundremann",
        [$"Achievement_{AchievementCatalog.CenturionId}_Description"] = "Fullførte 100 stå-sykluser.",
        [$"Achievement_{AchievementCatalog.LongHaulId}_Title"] = "Lang vei",
        [$"Achievement_{AchievementCatalog.LongHaulId}_Description"] = "Ti kumulative timer med stå. Ryggen din takker deg.",
        [$"Achievement_{AchievementCatalog.MountaineerId}_Title"] = "Fjellklatrer",
        [$"Achievement_{AchievementCatalog.MountaineerId}_Description"] = "Ett hundre kumulative timer med stå.",
    };

    public static readonly IReadOnlyDictionary<string, string[]> TextPools = new Dictionary<string, string[]>
    {
        ["SitDown"] =
        [
            "Du kan sette deg nå.",
            "Hvil beina.",
            "Sett deg ned og slapp av.",
            "Sett deg bekvemt.",
            "Ta plass.",
            "Ok, sett deg.",
            "På tide å sette seg.",
            "Hent stolen.",
            "Du kan ta en pause nå.",
            "Rumpa på stolen.",
        ],

        ["StandUp"] =
        [
            "Reis deg nå!",
            "Opp, opp, opp!",
            "Hev pulten!",
            "Reis deg, reis deg!",
            "Stå rak!",
            "På tide å reise seg!",
            "Skift til ståposisjon!",
            "Opp, soldat!",
            "Opp, lathans!",
            "Opp med deg!",
        ],

        ["NewDay"] =
        [
            "Ny dag!",
            "Frisk start.",
            "God morgen!",
            "Stå opp og strål.",
            "Enda en runde rundt solen.",
            "Dag én. Igjen.",
            "Blanke ark.",
            "Her går vi igjen.",
            "Dagen er nullstilt.",
            "Videre!",
        ],
    };
}
