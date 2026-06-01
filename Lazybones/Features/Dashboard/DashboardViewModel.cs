using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Lazybones.Core.Mvvm;
using Lazybones.Features.Achievements;
using Lazybones.Features.History;
using Lazybones.Core.State;
using Lazybones.Features.StartAtLogin;
using Lazybones.Features.Updates;
using Lazybones.Localization;

namespace Lazybones.Features.Dashboard;

public class DashboardViewModel : ViewModelBase, IDisposable
{
    private bool _disposed;

    public const int StatsTabIndex = 0;
    public const int AchievementsTabIndex = 1;
    public const int SettingsTabIndex = 2;
    public const int UpdatesTabIndex = 3;

    private readonly AppState _state;
    private readonly IHistoryStore _history;
    private readonly Action _onDailyGoalChanged;
    private readonly Action _onAlwaysOnTopChanged;
    private readonly UpdateService _updates = UpdateService.Instance;
    private readonly LocalizationService _loc = LocalizationService.Instance;
    private int _selectedTabIndex;
    private IReadOnlyList<AchievementViewItem> _achievements = [];
    private readonly ObservableCollection<string> _languageOptions = new();

    public DashboardViewModel(AppState state, IHistoryStore history, Action onDailyGoalChanged, Action onAlwaysOnTopChanged)
    {
        _state = state;
        _history = history;
        _onDailyGoalChanged = onDailyGoalChanged;
        _onAlwaysOnTopChanged = onAlwaysOnTopChanged;

        _achievements = BuildAchievements();
        RefreshLanguageOptions();

        HeatmapData = BuildHeatmap();
        CyclesPerDay = BuildCyclesPerDay();

        CheckForUpdatesCommand = new RelayCommand(() => _ = _updates.CheckAsync());
        RestartNowCommand = new RelayCommand(_updates.ApplyAndRestart);

        _updates.PropertyChanged += OnUpdateServicePropertyChanged;
        _loc.CultureChanged += OnCultureChanged;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _updates.PropertyChanged -= OnUpdateServicePropertyChanged;
        _loc.CultureChanged -= OnCultureChanged;
    }

    // Swapped for a fresh instance on culture change (see OnCultureChanged):
    // compiled bindings only re-read {Binding Strings.*} when the Strings
    // reference itself changes, since the leaf objects don't raise INPC.
    public DashboardStrings Strings { get; private set; } = new();

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => SetField(ref _selectedTabIndex, value);
    }

    public string CurrentVersionText => $"v{_updates.CurrentVersion}";

    public string UpdateStatusLabel => _updates.State switch
    {
        UpdateState.UpdateReady => _loc.Format("Updates_StatusReady", _updates.AvailableVersion ?? ""),
        UpdateState.Checking => _loc.Get("Updates_StatusChecking"),
        UpdateState.Failed => _loc.Get("Updates_StatusFailed"),
        _ => _loc.Get("Updates_StatusGeneric"),
    };

    public string UpdateStatusText
    {
        get
        {
            if (!_updates.CanUpdate)
                return _loc.Get("Updates_DevBuildText");
            return _updates.State switch
            {
                UpdateState.Idle => _loc.Get("Updates_IdleText"),
                UpdateState.Checking => _loc.Get("Updates_CheckingText"),
                UpdateState.UpToDate => _loc.Get("Updates_UpToDateText"),
                UpdateState.UpdateReady => _loc.Format("Updates_ReadyTextFormat", _updates.AvailableVersion ?? ""),
                UpdateState.Failed => _updates.ErrorMessage ?? _loc.Get("Updates_FailedText"),
                _ => string.Empty
            };
        }
    }

    public bool HasUpdateReady => _updates.State == UpdateState.UpdateReady;

    public bool CanCheckForUpdates => _updates.CanUpdate && _updates.State != UpdateState.Checking;

    public string ReleaseNotesText => _updates.ReleaseNotesMarkdown ?? _loc.Get("Updates_NotesLoading");

    public ICommand CheckForUpdatesCommand { get; }
    public ICommand RestartNowCommand { get; }

    private void OnUpdateServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Every UpdateService property change can affect the derived labels, so
        // just re-raise the lot rather than tracking which depends on which.
        OnPropertyChanged(nameof(CurrentVersionText));
        OnPropertyChanged(nameof(UpdateStatusLabel));
        OnPropertyChanged(nameof(UpdateStatusText));
        OnPropertyChanged(nameof(HasUpdateReady));
        OnPropertyChanged(nameof(CanCheckForUpdates));
        OnPropertyChanged(nameof(ReleaseNotesText));
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        // Rebuild the achievement view items so their snapshotted Title/Description
        // pick up the new language; raise PropertyChanged on every property whose
        // string contents depend on the active culture. Note: don't re-raise on
        // LanguageOptions — it's an in-place-mutated ObservableCollection, and
        // replacing its identity through INPC would make the ComboBox flicker
        // through SelectedIndex=-1 and feed that back into LanguageIndex.
        _achievements = BuildAchievements();
        RefreshLanguageOptions();
        // Swap in a fresh Strings instance so every {Binding Strings.*} re-reads
        // in the new language — a same-reference notify wouldn't re-read leaves.
        Strings = new DashboardStrings();
        OnPropertyChanged(nameof(Strings));
        OnPropertyChanged(nameof(Achievements));
        OnPropertyChanged(nameof(UnlockedSummary));
        OnPropertyChanged(nameof(TodayProgressText));
        OnPropertyChanged(nameof(TodayMinutesText));
        OnPropertyChanged(nameof(UpdateStatusLabel));
        OnPropertyChanged(nameof(UpdateStatusText));
        OnPropertyChanged(nameof(ReleaseNotesText));
    }

    public int StandingTime
    {
        get => _state.StandingTimeInMinutes;
        set
        {
            if (_state.StandingTimeInMinutes == value) return;
            _state.StandingTimeInMinutes = value;
            _state.SaveState();
            OnPropertyChanged(nameof(StandingTime));
        }
    }

    public int SittingTime
    {
        get => _state.SittingTimeInMinutes;
        set
        {
            if (_state.SittingTimeInMinutes == value) return;
            _state.SittingTimeInMinutes = value;
            _state.SaveState();
            OnPropertyChanged(nameof(SittingTime));
        }
    }

    public int DailyCycleGoal
    {
        get => _state.DailyCycleGoal;
        set
        {
            if (_state.DailyCycleGoal == value) return;
            _state.DailyCycleGoal = value;
            _state.SaveState();
            OnPropertyChanged(nameof(DailyCycleGoal));
            OnPropertyChanged(nameof(TodayProgressText));
            OnPropertyChanged(nameof(DailyMinuteThreshold));
            _onDailyGoalChanged();
        }
    }

    // Two ints decomposed from the persisted TimeSpan, bound to ClockDial's
    // Hour / Minute. RolloverTimeText is the formatted face shown on the
    // dropdown button — must re-raise whenever either component changes.
    public int RolloverHour
    {
        get => _state.DayRolloverTime.Hours;
        set
        {
            var clamped = ((value % 24) + 24) % 24;
            if (_state.DayRolloverTime.Hours == clamped) return;
            _state.DayRolloverTime = new TimeSpan(clamped, _state.DayRolloverTime.Minutes, 0);
            _state.SaveState();
            OnPropertyChanged(nameof(RolloverHour));
            OnPropertyChanged(nameof(RolloverTimeText));
        }
    }

    public int RolloverMinute
    {
        get => _state.DayRolloverTime.Minutes;
        set
        {
            var clamped = ((value % 60) + 60) % 60;
            if (_state.DayRolloverTime.Minutes == clamped) return;
            _state.DayRolloverTime = new TimeSpan(_state.DayRolloverTime.Hours, clamped, 0);
            _state.SaveState();
            OnPropertyChanged(nameof(RolloverMinute));
            OnPropertyChanged(nameof(RolloverTimeText));
        }
    }

    public string RolloverTimeText =>
        $"{_state.DayRolloverTime.Hours:00}:{_state.DayRolloverTime.Minutes:00}";

    // Bound to a ComboBox's SelectedIndex: 0 = seated, 1 = standing. Kept as
    // a derived view over the bool in AppState so persistence semantics stay
    // unchanged.
    public int StartDayModeIndex
    {
        get => _state.StartDayStanding ? 1 : 0;
        set
        {
            var standing = value == 1;
            if (_state.StartDayStanding == standing) return;
            _state.StartDayStanding = standing;
            _state.SaveState();
            OnPropertyChanged(nameof(StartDayModeIndex));
        }
    }

    // Language picker: index 0 = follow OS, 1..n = explicit language codes from
    // LocalizationService.AvailableLanguages in order. The "Auto" entry is
    // localized; the explicit-language entries use each language's native name
    // so a user can find their own language regardless of the active UI.
    //
    // The collection itself is set once and mutated in place — replacing its
    // identity on culture change makes the ComboBox briefly clear its items
    // and push SelectedIndex=-1 back through the binding, which corrupts the
    // stored preference.
    public ObservableCollection<string> LanguageOptions => _languageOptions;

    private void RefreshLanguageOptions()
    {
        var autoLabel = _loc.Get("Settings_LanguageAuto");
        if (_languageOptions.Count == 0)
        {
            _languageOptions.Add(autoLabel);
            foreach (var lang in _loc.AvailableLanguages)
                _languageOptions.Add(lang.DisplayName);
        }
        else
        {
            // Native language names don't translate; only the Auto entry shifts.
            _languageOptions[0] = autoLabel;
        }
    }

    public int LanguageIndex
    {
        get
        {
            if (string.IsNullOrEmpty(_state.Language)) return 0;
            var languages = _loc.AvailableLanguages;
            for (var i = 0; i < languages.Count; i++)
                if (string.Equals(languages[i].Code, _state.Language, StringComparison.OrdinalIgnoreCase))
                    return i + 1;
            return 0;
        }
        set
        {
            // Guard the transient -1 the ComboBox pushes during any items-source
            // change; treating that as a real selection wipes the preference.
            if (value < 0) return;

            string? preference = value == 0
                ? null
                : _loc.AvailableLanguages.ElementAtOrDefault(value - 1).Code;
            if (preference == _state.Language) return;
            _state.Language = preference;
            _state.SaveState();
            _loc.Apply(preference);
            OnPropertyChanged(nameof(LanguageIndex));
        }
    }

    // Derived from cycle goal × cycle length — the implicit minute equivalent
    // of your daily commitment. The heatmap uses this to color cells.
    public int DailyMinuteThreshold => _state.DailyCycleGoal * _state.StandingTimeInMinutes;

    public bool StartWithWindows
    {
        get => _state.StartWithWindows;
        set
        {
            if (_state.StartWithWindows == value) return;
            // Persist what the OS actually did, not what the user clicked, so
            // the toggle reflects reality after restart.
            var applied = StartupService.Instance.SetEnabled(value) && value;
            _state.StartWithWindows = applied;
            _state.SaveState();
            OnPropertyChanged(nameof(StartWithWindows));
        }
    }

    public bool AlwaysOnTop
    {
        get => _state.AlwaysOnTop;
        set
        {
            if (_state.AlwaysOnTop == value) return;
            _state.AlwaysOnTop = value;
            _state.SaveState();
            OnPropertyChanged(nameof(AlwaysOnTop));
            _onAlwaysOnTopChanged();
        }
    }

    private DateOnly Today => LogicalDay.From(DateTime.Now, _state.DayRolloverTime);

    public int TodayStandingMinutes => _history.StandingMinutesOn(Today, _state.DayRolloverTime);
    public int TodayStandingCycles => _history.CompletedStandingCyclesOn(Today, _state.DayRolloverTime);

    public string TodayProgressText => _loc.Format("Stats_TodayProgressFormat", TodayStandingCycles, DailyCycleGoal);
    public string TodayMinutesText => _loc.Format("Stats_TodayMinutesFormat", TodayStandingMinutes);

    public int CurrentStreak => StreakCalculator.CalculateCurrent(
        _history, _state.DailyCycleGoal, Today, _state.DayRolloverTime);

    public IReadOnlyDictionary<DateOnly, int> HeatmapData { get; }

    public IReadOnlyList<int> CyclesPerDay { get; }

    public IReadOnlyList<AchievementViewItem> Achievements => _achievements;

    public int UnlockedCount => _achievements.Count(a => a.IsUnlocked);

    public string UnlockedSummary => _loc.Format("Achievements_UnlockedSummary", UnlockedCount, _achievements.Count);

    private List<AchievementViewItem> BuildAchievements() =>
        AchievementCatalog.All
            .Select(a => new AchievementViewItem(a, _state.UnlockedAchievementIds.Contains(a.Id)))
            .ToList();

    private Dictionary<DateOnly, int> BuildHeatmap()
    {
        var rollover = _state.DayRolloverTime;
        var today = LogicalDay.From(DateTime.Now, rollover);
        var dow = ((int)today.DayOfWeek + 6) % 7;
        var lastMonday = today.AddDays(-dow);
        var firstMonday = lastMonday.AddDays(-12 * 7);
        var records = _history.GetRange(firstMonday, today, rollover);

        var data = new Dictionary<DateOnly, int>();
        foreach (var r in records)
        {
            if (!r.WasStanding) continue;
            var d = LogicalDay.From(r.EndedAt, rollover);
            data[d] = data.GetValueOrDefault(d, 0) + r.ActualDurationSeconds / 60;
        }
        return data;
    }

    private int[] BuildCyclesPerDay()
    {
        const int days = 14;
        var rollover = _state.DayRolloverTime;
        var today = LogicalDay.From(DateTime.Now, rollover);
        var start = today.AddDays(-(days - 1));
        var records = _history.GetRange(start, today, rollover);

        var result = new int[days];
        foreach (var r in records)
        {
            if (!r.WasStanding || r.Outcome != CycleOutcome.CompletedNaturally) continue;
            var day = LogicalDay.From(r.StartedAt, rollover);
            var index = day.DayNumber - start.DayNumber;
            if (index >= 0 && index < days) result[index]++;
        }
        return result;
    }
}

public sealed class AchievementViewItem
{
    public AchievementViewItem(Achievement achievement, bool isUnlocked)
    {
        // Snapshot the localized strings at construction; DashboardViewModel
        // rebuilds the whole list on culture change, so a stale snapshot can
        // never be displayed.
        Title = achievement.Title;
        Description = achievement.Description;
        IsUnlocked = isUnlocked;
    }

    public string Title { get; }
    public string Description { get; }
    public bool IsUnlocked { get; }
}
