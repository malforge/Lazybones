using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Lazybones.Core.Mvvm;
using Lazybones.Core.State;
using Lazybones.Features.Achievements;
using Lazybones.Features.Dashboard;
using Lazybones.Features.History;
using Lazybones.Features.SessionPresence;
using Lazybones.Features.StartAtLogin;
using Lazybones.Features.Updates;
using Lazybones.Localization;

namespace Lazybones.Features.Shell;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly LocalizationService _loc = LocalizationService.Instance;

    private string _hours = "00";
    private string _minutes = "00";
    private string _seconds = "00";
    private TimeSpan _time;
    private bool _isStanding;
    private string _text = string.Empty;

    private readonly Stopwatch _stopwatch = new();
    private bool _isRunning;
    private readonly AppState _state;
    private readonly IHistoryStore _history;
    private Point _windowPosition;
    private readonly OverlayViewModel _overlay = new();
    private readonly DispatcherTimer _timer;
    private bool _triggerInFlight;
    // The end-of-cycle change-position dialog while it's open, plus a flag set
    // when a day rollover dismisses it out from under the user — so TriggerAsync's
    // continuation knows the rollover already handled (and credited) the cycle.
    private ModeSwitchDialog? _activeModeSwitchDialog;
    private bool _rolloverInterruptedTrigger;
    private DateTime _cycleStartedAt;
    // Flips to true the first time the user confirms a time-adjust during the
    // current cycle. Reset at every cycle boundary. Copied onto the CycleRecord
    // in RecordCurrentCycle; downstream filters (achievement evaluation,
    // streak, today's ring) treat tainted cycles as non-counting.
    private bool _currentCycleTimeEdited;
    private int _cyclePlannedSeconds;
    // Wall-clock time at which the most recent TriggerAsync() fired (= when the
    // timer actually hit zero). Used to measure dialog response time without
    // counting any pre-trigger pause window.
    private DateTime _lastTriggerFiredAt;
    private double _outerRingProgress;
    private double _innerRingProgress;
    private int _streak;
    private DateOnly _currentDay;
    private bool _autoPaused;
    private DateTime _autoPauseStartedAt;
    private readonly IUserPresenceMonitor _presence = UserPresenceMonitor.Create();

    // Decorative clock-face background. _clockFaceImage is the loaded bitmap (or
    // null); _loadedClockFacePath tracks which path it came from so a slider
    // change doesn't pointlessly reload from disk.
    private Bitmap? _clockFaceImage;
    private string? _loadedClockFacePath;

    public MainWindowViewModel() : this(new HistoryStore()) { }

    public MainWindowViewModel(IHistoryStore history)
    {
        _history = history;
        _state = AppState.LoadState();

        PlayPauseCommand = new RelayCommand(PlayPause);
        ResetCommand = new RelayCommand(ConfirmReset);
        SwapCommand = new RelayCommand(ConfirmToggle);
        DashboardCommand = new RelayCommand(() => ShowDashboard());
        OpenUpdatesCommand = new RelayCommand(() => ShowDashboard(DashboardViewModel.UpdatesTabIndex));
        OpenStatsCommand = new RelayCommand(() => ShowDashboard(DashboardViewModel.StatsTabIndex));
        OpenAchievementsCommand = new RelayCommand(() => ShowDashboard(DashboardViewModel.AchievementsTabIndex));
        AdjustTimeCommand = new RelayCommand(ShowTimeAdjustment);
        ConfirmOverlayCommand = new RelayCommand(() => _overlay.Confirm());
        CancelOverlayCommand = new RelayCommand(() => _overlay.Cancel());

        UpdateService.Instance.PropertyChanged += OnUpdateServicePropertyChanged;
        UpdateService.Instance.StartPolling();
        _loc.CultureChanged += OnCultureChanged;

        // Validate position - if minimized (-32000) or off-screen, use default position
        var left = _state.Left ?? 100;
        var top = _state.Top ?? 100;
        if (left < -1000 || top < -1000 || left > 10000 || top > 10000)
        {
            left = 100;
            top = 100;
        }

        WindowPosition = new Point(left, top);
        IsRunning = _state.IsRunning;
        IsStanding = _state.IsStanding;

        // Set text based on mode
        Text = IsStanding
            ? _loc.Pick("StandUp")
            : _loc.Pick("SitDown");

        // Restore timer from saved state, or use defaults
        if (_state.ElapsedTimeInSeconds > 0)
        {
            Time = TimeSpan.FromSeconds(_state.ElapsedTimeInSeconds);
        }
        else
        {
            Time = TimeSpan.FromMinutes(IsStanding ? _state.StandingTimeInMinutes : _state.SittingTimeInMinutes);
        }

        if (IsRunning)
            _stopwatch.Start();

        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, OnTimerTick);
        _timer.Start();

        _currentDay = LogicalDay.From(DateTime.Now, _state.DayRolloverTime);
        StartNewCycle(persist: false);
        // If the previous session was mid-cycle, restore the cycle's identity
        // metadata over the fresh values StartNewCycle just set. Without this,
        // close-and-reopen mid-cycle re-anchors StartedAt to "now" (breaking
        // hour-of-day achievements) and washes the time-edit taint flag.
        if (_state.CycleStartedAt.HasValue)
        {
            _cycleStartedAt = _state.CycleStartedAt.Value;
            _currentCycleTimeEdited = _state.CurrentCycleTimeEdited;
            // StartNewCycle wiped the pause collection assuming a fresh cycle;
            // for a restored cycle we keep whatever pauses had accumulated.
        }
        _state.CurrentCyclePauses ??= new();

        // Close out any pause that was open at last shutdown (graceful or not):
        // an explicitly-recorded AppShutdown pause from Dispose, an in-flight
        // ScreenLock the user was in when we exited, or a fallback pause
        // synthesized from the AppLastAliveAt heartbeat when no in-progress
        // pause was on record (covers crashes / kill / Environment.Exit).
        if (_state.CurrentPauseStartedAt.HasValue)
        {
            EndPauseInterval();
        }
        else if (_state.AppLastAliveAt.HasValue && _state.CycleStartedAt.HasValue)
        {
            var gap = DateTime.Now - _state.AppLastAliveAt.Value;
            if (gap.TotalSeconds >= 30)
            {
                _state.CurrentCyclePauses.Add(new PauseInterval
                {
                    StartedAt = _state.AppLastAliveAt.Value,
                    EndedAt = DateTime.Now,
                    Reason = PauseReason.AppShutdown
                });
            }
        }
        _state.AppLastAliveAt = DateTime.Now;
        RefreshOuterRing();
        RefreshInnerRing();
        RefreshStreak();

        _presence.Locked += OnScreenLocked;
        _presence.Unlocked += OnScreenUnlocked;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        // Load the saved clock-face background, if any. A missing/unreadable
        // file surfaces a toast (once, at startup) and is otherwise ignored.
        LoadClockFaceImage(showToastOnError: true);

        // Apply any day rollover that should have fired while the app was
        // closed. First-run anchors silently (no toast).
        ApplyDayRolloverIfDue(silent: !_state.LastRolloverAppliedAt.HasValue);

        if (!_state.HasShownInitialSettings)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ShowDashboard(DashboardViewModel.SettingsTabIndex);
                _state.HasShownInitialSettings = true;
                _state.SaveState();
            }, DispatcherPriority.Background);
        }

        // Construction complete — any further upward Streak transitions are
        // real user accomplishments, not "restored from disk".
        _suppressStreakCelebration = false;
    }

    // Returns the most recent rollover boundary at or before `now` for the
    // configured rollover time-of-day. E.g. with time=06:30 and now=
    // 2026-05-28 04:30, returns 2026-05-27 06:30.
    private DateTime MostRecentRolloverBoundary(DateTime now)
    {
        var tod = _state.DayRolloverTime;
        if (tod < TimeSpan.Zero || tod >= TimeSpan.FromDays(1))
            tod = TimeSpan.FromHours(6);
        var todayBoundary = now.Date + tod;
        return now >= todayBoundary ? todayBoundary : todayBoundary.AddDays(-1);
    }

    // Checks whether a rollover boundary has passed since LastRolloverAppliedAt
    // and, if so, resets the cycle to the configured start mode and shows the
    // toast. When `silent` is true (first run), just anchors the timestamp.
    // Returns true when a visible rollover was applied (caller can suppress
    // any competing toast).
    private bool ApplyDayRolloverIfDue(bool silent = false)
    {
        var boundary = MostRecentRolloverBoundary(DateTime.Now);
        var last = _state.LastRolloverAppliedAt;
        if (last.HasValue && last.Value >= boundary) return false;

        _state.LastRolloverAppliedAt = boundary;

        if (silent)
        {
            _state.SaveState();
            return false;
        }

        // Close out the in-progress cycle. Normally a RolloverReset (NOT a manual
        // Reset) so the streak and other "active interaction" checks ignore it —
        // the user is never punished for the app's own day rollover.
        //
        // Exception: a cycle whose timer already hit zero is sitting behind an
        // open change-position dialog (the user stepped away before answering).
        // That cycle genuinely completed, so credit it as CompletedNaturally,
        // dismiss the now-stale dialog, and flag TriggerAsync to bail so it
        // doesn't re-record or toggle on top of the reset below.
        if (_triggerInFlight && _activeModeSwitchDialog != null)
        {
            RecordCurrentCycle(CycleOutcome.CompletedNaturally);
            _rolloverInterruptedTrigger = true;
            _activeModeSwitchDialog.Close();
        }
        else
        {
            RecordCurrentCycle(CycleOutcome.RolloverReset);
        }

        var startStanding = _state.StartDayStanding;
        if (startStanding) StandUp(); else SitDown();

        // A rollover that interrupted an open change-position prompt must leave
        // the new day's cycle RUNNING. The prompt being up only means a cycle
        // completed — the timer was auto-paused by TriggerAsync, not by the
        // user — so we read it as "continue", matching a rollover that fires
        // mid-run (which keeps running across the boundary). Resume() no-ops if
        // already running, so the normal rollover path is unaffected.
        if (_rolloverInterruptedTrigger) Resume();

        var actionText = startStanding
            ? _loc.Pick("StandUp")
            : _loc.Pick("SitDown");
        _overlay.ShowDayRollover(_loc.Pick("NewDay"), actionText);
        return true;
    }

    private void StartNewCycle(bool persist = true)
    {
        _cycleStartedAt = DateTime.Now;
        _currentCycleTimeEdited = false;
        _cyclePlannedSeconds = (IsStanding ? _state.StandingTimeInMinutes : _state.SittingTimeInMinutes) * 60;
        // Fresh cycle starts with no pauses; any in-progress pause from the
        // previous cycle is dropped (cycle transitions imply the user is back).
        _state.CurrentCyclePauses = new();
        _state.CurrentPauseStartedAt = null;
        _state.CurrentPauseReason = null;
        RefreshInnerRing();

        // Flush the new mode/cycle to disk now, not just on Dispose: a swap is
        // a deliberate state change, and several exit paths skip Dispose — a
        // crash, an OS restart, or an update-driven ApplyAndRestart. Without
        // this, such an exit resumes in the pre-swap mode and re-runs a cycle
        // that already completed. Skipped during construction (persist: false),
        // where the saved cycle identity is restored right after this call and
        // must not be overwritten with a fresh StartedAt.
        if (persist)
            PersistCycleState();
    }

    // Writes the live mode/cycle fields through to AppState and saves. Mirrors
    // the set persisted on Dispose; SaveState swallows transient I/O errors.
    private void PersistCycleState()
    {
        _state.ElapsedTimeInSeconds = (int)Time.TotalSeconds;
        _state.IsRunning = IsRunning;
        _state.IsStanding = IsStanding;
        _state.CycleStartedAt = _cycleStartedAt;
        _state.CurrentCycleTimeEdited = _currentCycleTimeEdited;
        _state.SaveState();
    }

    private void RecordCurrentCycle(CycleOutcome outcome, bool promptDismissed = false)
    {
        // Clamp elapsed against planned so a user-driven Time adjustment can't
        // produce negative or absurd actual-duration values.
        var remaining = (int)Math.Max(0, Time.TotalSeconds);
        var actual = Math.Clamp(_cyclePlannedSeconds - remaining, 0, _cyclePlannedSeconds);

        // Logical end time = StartedAt + active time. By construction this
        // excludes all pause windows (locked, manual, dialog wait), so pause
        // time never shifts day buckets, hour-of-day achievements, or any
        // other time-derived metric.
        var endedAt = _cycleStartedAt.AddSeconds(actual);

        int responseDelaySeconds;
        if (outcome == CycleOutcome.CompletedNaturally)
        {
            // Time between the dialog actually firing (wall clock, captured in
            // Trigger) and the user closing it. Pause time before the trigger
            // doesn't count.
            responseDelaySeconds = (int)Math.Max(0, (DateTime.Now - _lastTriggerFiredAt).TotalSeconds);
        }
        else
        {
            responseDelaySeconds = 0;
        }

        // Any pause that's still in flight at cycle-end gets closed off here
        // (e.g. cycle finishes naturally while the screen is still locked —
        // unlikely but possible).
        EndPauseInterval();

        var record = new CycleRecord
        {
            StartedAt = _cycleStartedAt,
            EndedAt = endedAt,
            WasStanding = IsStanding,
            PlannedDurationSeconds = _cyclePlannedSeconds,
            ActualDurationSeconds = actual,
            Outcome = outcome,
            PromptDismissed = promptDismissed,
            ResponseDelaySeconds = responseDelaySeconds,
            WasTimeEdited = _currentCycleTimeEdited,
            Pauses = _state.CurrentCyclePauses?.ToList() ?? new()
        };
        _history.Append(record);

        // Pauses now live on the record; reset the in-flight collection.
        _state.CurrentCyclePauses = new();

        RefreshOuterRing();
        RefreshStreak();
        EvaluateAchievements(record);
    }

    private void EvaluateAchievements(CycleRecord record)
    {
        var today = LogicalDay.From(DateTime.Now, _state.DayRolloverTime);
        var newly = AchievementRules.EvaluateNewlyUnlocked(
            record, _history, _state.DailyCycleGoal, _state.UnlockedAchievementIds, today, _state.DayRolloverTime);
        if (newly.Count == 0) return;

        foreach (var ach in newly)
        {
            _state.UnlockedAchievementIds.Add(ach.Id);
            _overlay.QueueAchievement(ach);
        }
        _state.SaveState();

        OnPropertyChanged(nameof(AchievementProgressText));
    }

    private bool _ringLatchedFull;
    private DispatcherTimer? _ringLatchTimer;

    private void RefreshOuterRing()
    {
        if (_ringLatchedFull)
        {
            OuterRingProgress = 1.0;
            return;
        }
        var goal = Math.Max(1, _state.DailyCycleGoal);
        var today = LogicalDay.From(DateTime.Now, _state.DayRolloverTime);
        var cyclesToday = _history.CompletedStandingCyclesOn(today, _state.DayRolloverTime);
        // Loop every `goal` cycles: completing the goal resets the ring so it
        // fills again for the next streak. The latch above briefly holds the
        // ring at 100% so the user sees the "full" moment before reset.
        OuterRingProgress = (double)(cyclesToday % goal) / goal;
    }

    // Latches the outer ring at 100% for a moment, then snaps it to the
    // start of the next round. Called when a streak event lands so the
    // confetti has a "full ring" to celebrate against.
    private void LatchRingFull(TimeSpan duration)
    {
        _ringLatchedFull = true;
        _ringLatchTimer?.Stop();
        OuterRingProgress = 1.0;
        _ringLatchTimer = new DispatcherTimer(duration, DispatcherPriority.Normal, (_, _) =>
        {
            _ringLatchTimer?.Stop();
            _ringLatchTimer = null;
            _ringLatchedFull = false;
            RefreshOuterRing();
        });
        _ringLatchTimer.Start();
    }

    private void RefreshStreak()
    {
        var today = LogicalDay.From(DateTime.Now, _state.DayRolloverTime);
        Streak = StreakCalculator.CalculateCurrent(_history, _state.DailyCycleGoal, today, _state.DayRolloverTime);
    }

    // Fires when the streak count crosses upward — i.e. the user just landed a
    // new completion. The View subscribes to celebrate (confetti + ring latch).
    public event Action? StreakAdvanced;
    private bool _suppressStreakCelebration = true;

    public int Streak
    {
        get => _streak;
        private set
        {
            var previous = _streak;
            if (!SetField(ref _streak, value)) return;
            OnPropertyChanged(nameof(HasStreak));
            if (value > previous && !_suppressStreakCelebration)
            {
                LatchRingFull(TimeSpan.FromMilliseconds(900));
                StreakAdvanced?.Invoke();
            }
        }
    }

    public bool HasStreak => _streak > 0;

    private void RefreshInnerRing()
    {
        if (_cyclePlannedSeconds <= 0)
        {
            InnerRingProgress = 0;
            return;
        }
        var remaining = Math.Max(0, Time.TotalSeconds);
        InnerRingProgress = Math.Clamp(1.0 - remaining / _cyclePlannedSeconds, 0, 1);
    }

    public double OuterRingProgress
    {
        get => _outerRingProgress;
        private set => SetField(ref _outerRingProgress, value);
    }

    public double InnerRingProgress
    {
        get => _innerRingProgress;
        private set => SetField(ref _innerRingProgress, value);
    }

    public ICommand PlayPauseCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SwapCommand { get; }
    public ICommand DashboardCommand { get; }
    public ICommand OpenUpdatesCommand { get; }
    public ICommand OpenStatsCommand { get; }
    public ICommand OpenAchievementsCommand { get; }
    public ICommand AdjustTimeCommand { get; }
    public ICommand ConfirmOverlayCommand { get; }
    public ICommand CancelOverlayCommand { get; }

    public bool HasUpdate => UpdateService.Instance.State == UpdateState.UpdateReady;

    public string AvailableVersionText => $"v{UpdateService.Instance.AvailableVersion}";

    public int UnlockedAchievementCount => _state.UnlockedAchievementIds.Count;
    public int TotalAchievementCount => AchievementCatalog.All.Count;
    public string AchievementProgressText => $"{UnlockedAchievementCount}/{TotalAchievementCount}";

    private void OnUpdateServicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UpdateService.State))
            OnPropertyChanged(nameof(HasUpdate));
        if (e.PropertyName == nameof(UpdateService.AvailableVersion))
            OnPropertyChanged(nameof(AvailableVersionText));
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        // Pull a fresh text in the new language from whichever pool/key matches
        // the current mode. The "Locked..." pause state isn't distinguished
        // from manual pause here — that's rare enough that falling back to the
        // generic Mode_Paused string when not running is acceptable; the next
        // OnScreenUnlocked tick sets the proper text.
        Text = IsRunning
            ? _loc.Pick(IsStanding ? "StandUp" : "SitDown")
            : _loc.Get("Mode_Paused");

        // Swap in a fresh Strings instance so every {Binding Strings.*}
        // (tooltips, dialog buttons, toasts) re-reads in the new language — a
        // same-reference notify wouldn't re-read leaves.
        Strings = new MainWindowStrings();
        OnPropertyChanged(nameof(Strings));
    }

    // Swapped for a fresh instance on culture change (see OnCultureChanged):
    // compiled bindings only re-read {Binding Strings.*} when the reference
    // changes, since the leaf objects don't raise INPC.
    public MainWindowStrings Strings { get; private set; } = new();

    // -- Clock-face background ---------------------------------------------
    public Bitmap? ClockFaceImage => _clockFaceImage;
    public bool HasClockFaceImage => _clockFaceImage != null;
    public double ClockFaceImageScale => _state.ClockFaceImageScale;
    public double ClockFaceImageAlpha => _state.ClockFaceImageAlpha;

    // Pan offset, in disk pixels, for an image scaled past the face. The scale
    // is applied about the center, so panning is just a translation: the image
    // overflows the face by (scale-1) * diameter total, split each side, and the
    // 0..1 slider walks across that range — offset 0 reveals the left/top edge,
    // 1 the right/bottom, 0.5 stays centered. Zero overflow at scale <= 1.
    private const double DiskDiameter = 260;
    public double ClockFaceImageTranslateX =>
        (0.5 - _state.ClockFaceImageOffsetX) * (_state.ClockFaceImageScale - 1) * DiskDiameter;
    public double ClockFaceImageTranslateY =>
        (0.5 - _state.ClockFaceImageOffsetY) * (_state.ClockFaceImageScale - 1) * DiskDiameter;

    private void LoadClockFaceImage(bool showToastOnError)
    {
        _loadedClockFacePath = _state.ClockFaceImagePath;

        var previous = _clockFaceImage;
        _clockFaceImage = null;

        var path = _state.ClockFaceImagePath;
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                if (!File.Exists(path)) throw new FileNotFoundException(path);
                _clockFaceImage = new Bitmap(path);
            }
            catch (Exception)
            {
                // Bad path, unreadable file, or not a decodable image — drop it
                // and let the user know, but never block startup over decoration.
                _clockFaceImage = null;
                if (showToastOnError)
                    _overlay.ShowNotice(_loc.Get("Toast_ImageLoadFailedTitle"),
                                        _loc.Get("Toast_ImageLoadFailed"));
            }
        }

        previous?.Dispose();
        OnPropertyChanged(nameof(ClockFaceImage));
        OnPropertyChanged(nameof(HasClockFaceImage));
    }

    // Called from the dashboard when the clock-face settings change. Reloads the
    // bitmap only when the path actually changed (slider drags don't touch it),
    // and always re-raises scale/alpha so the disk updates live.
    public void ReloadClockFace()
    {
        if (_state.ClockFaceImagePath != _loadedClockFacePath)
            LoadClockFaceImage(showToastOnError: true);
        OnPropertyChanged(nameof(ClockFaceImageScale));
        OnPropertyChanged(nameof(ClockFaceImageAlpha));
        OnPropertyChanged(nameof(ClockFaceImageTranslateX));
        OnPropertyChanged(nameof(ClockFaceImageTranslateY));
    }

    public OverlayViewModel Overlay => _overlay;

    public string Hours
    {
        get => _hours;
        private set => SetField(ref _hours, value);
    }

    public string Minutes
    {
        get => _minutes;
        private set => SetField(ref _minutes, value);
    }

    public string Seconds
    {
        get => _seconds;
        private set => SetField(ref _seconds, value);
    }

    public TimeSpan Time
    {
        get => _time;
        set
        {
            if (_time == value) return;
            _time = value;
            // Use total components so values >= 24h render correctly.
            var clamped = value < TimeSpan.Zero ? TimeSpan.Zero : value;
            Hours = ((int)clamped.TotalHours).ToString("00");
            Minutes = clamped.Minutes.ToString("00");
            Seconds = clamped.Seconds.ToString("00");
        }
    }

    public bool IsRunning
    {
        get => _isRunning;
        set => SetField(ref _isRunning, value);
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
        }
    }

    public bool IsStanding
    {
        get => _isStanding;
        private set => SetField(ref _isStanding, value);
    }

    public string Text
    {
        get => _text;
        private set => SetField(ref _text, value);
    }

    public Point WindowPosition
    {
        get => _windowPosition;
        set => SetField(ref _windowPosition, value);
    }

    private void PlayPause()
    {
        if (IsRunning)
            Pause(trackAs: PauseReason.ManualPause);
        else
            Resume();
    }

    private void Resume()
    {
        if (IsRunning) return;

        // Restart, not Start: Stopwatch.Start() resumes from the previously
        // stored Elapsed value, which would get subtracted from Time on the
        // next tick.
        _stopwatch.Restart();
        Text = IsStanding
            ? _loc.Pick("StandUp")
            : _loc.Pick("SitDown");
        IsRunning = true;
        EndPauseInterval();
    }

    private void Pause(string? pauseText = null, PauseReason? trackAs = null)
    {
        if (!IsRunning) return;
        // Reset (not Stop) so any residual elapsed is discarded; otherwise
        // Resume()'s next tick subtracts that residual from Time.
        _stopwatch.Reset();
        Text = pauseText ?? _loc.Get("Mode_Paused");
        IsRunning = false;
        if (trackAs.HasValue) StartPauseInterval(trackAs.Value);
    }

    private void StartPauseInterval(PauseReason reason)
    {
        // Only one pause may be in flight at a time — re-entrant pause sources
        // (e.g. lock fires while a manual pause is already open) keep the
        // earlier reason.
        if (_state.CurrentPauseStartedAt.HasValue) return;
        _state.CurrentPauseStartedAt = DateTime.Now;
        _state.CurrentPauseReason = reason;
        _state.SaveState();
    }

    private void EndPauseInterval()
    {
        if (!_state.CurrentPauseStartedAt.HasValue) return;
        _state.CurrentCyclePauses ??= new();
        _state.CurrentCyclePauses.Add(new PauseInterval
        {
            StartedAt = _state.CurrentPauseStartedAt.Value,
            EndedAt = DateTime.Now,
            Reason = _state.CurrentPauseReason ?? PauseReason.ManualPause
        });
        _state.CurrentPauseStartedAt = null;
        _state.CurrentPauseReason = null;
        _state.SaveState();
    }

    private void Reset()
    {
        ResetStopwatch();
        Time = IsStanding
            ? TimeSpan.FromMinutes(_state.StandingTimeInMinutes)
            : TimeSpan.FromMinutes(_state.SittingTimeInMinutes);
        StartNewCycle();
    }

    private void StandUp()
    {
        Text = _loc.Pick("StandUp");
        Time = TimeSpan.FromMinutes(_state.StandingTimeInMinutes);
        ResetStopwatch();
        IsStanding = true;
        StartNewCycle();
    }

    private void SitDown()
    {
        Text = _loc.Pick("SitDown");
        Time = TimeSpan.FromMinutes(_state.SittingTimeInMinutes);
        ResetStopwatch();
        IsStanding = false;
        StartNewCycle();
    }

    private void Toggle()
    {
        if (IsStanding)
            SitDown();
        else
            StandUp();
    }

    private async Task TriggerAsync()
    {
        // Re-entrancy guard: a slow dialog open shouldn't allow a second
        // Trigger to fire from a stale Time<=0 condition.
        if (_triggerInFlight) return;
        _triggerInFlight = true;
        // Snapshot the moment the cycle naturally ended (timer hit zero).
        // ResponseDelaySeconds is measured from here, not from the cycle's
        // logical end time, so pre-trigger pauses don't masquerade as a slow
        // response.
        _lastTriggerFiredAt = DateTime.Now;
        try
        {
            Pause();

            var dialog = new ModeSwitchDialog();
            var randomText = IsStanding
                ? _loc.Pick("SitDown")
                : _loc.Pick("StandUp");
            dialog.SetMessage(randomText);

            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow != null)
            {
                _activeModeSwitchDialog = dialog;
                await dialog.ShowDialog(mainWindow);
            }

            // A day rollover that fired while the dialog was open already
            // credited this cycle and reset the day; don't double-record or
            // toggle on top of it.
            if (_rolloverInterruptedTrigger)
                return;

            var promptDismissed = dialog.UserChoice != ModeSwitchDialog.Choice.StartNow
                                  && dialog.UserChoice != ModeSwitchDialog.Choice.Dismiss;
            RecordCurrentCycle(CycleOutcome.CompletedNaturally, promptDismissed);

            switch (dialog.UserChoice)
            {
                case ModeSwitchDialog.Choice.StartNow:
                    Toggle();
                    Resume();
                    break;
                case ModeSwitchDialog.Choice.Dismiss:
                    Toggle();
                    break;
                default:
                    // Closed without choosing (e.g. Alt+F4): keep current mode,
                    // but reset Time to the current mode's default so the timer
                    // doesn't immediately re-trigger when the user resumes.
                    Time = IsStanding
                        ? TimeSpan.FromMinutes(_state.StandingTimeInMinutes)
                        : TimeSpan.FromMinutes(_state.SittingTimeInMinutes);
                    StartNewCycle();
                    break;
            }
        }
        finally
        {
            _triggerInFlight = false;
            _activeModeSwitchDialog = null;
            _rolloverInterruptedTrigger = false;
        }
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _timer.Stop();
        _presence.Locked -= OnScreenLocked;
        _presence.Unlocked -= OnScreenUnlocked;
        _presence.Dispose();
        UpdateService.Instance.PropertyChanged -= OnUpdateServicePropertyChanged;
        _loc.CultureChanged -= OnCultureChanged;
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;

        _clockFaceImage?.Dispose();

        _state.Left = (int)WindowPosition.X;
        _state.Top = (int)WindowPosition.Y;

        // If a cycle is in flight, mark the start of a shutdown pause now.
        // The next launch's constructor will close it. If a pause is already
        // active (e.g. screen lock at shutdown time), leave it as-is — its
        // ScreenLock reason is more specific than AppShutdown.
        if (_state.CycleStartedAt.HasValue && !_state.CurrentPauseStartedAt.HasValue)
        {
            _state.CurrentPauseStartedAt = DateTime.Now;
            _state.CurrentPauseReason = PauseReason.AppShutdown;
        }
        _state.AppLastAliveAt = DateTime.Now;
        PersistCycleState();
    }

    // Last-ditch persistence for ungraceful exits (Environment.Exit from
    // Velopack, OS shutdown, etc.). Avalonia window-close events do NOT fire
    // on Environment.Exit, so Dispose may never run. ProcessExit at least
    // gives us a ~2 s window to flush the heartbeat + mark the shutdown pause.
    private void OnProcessExit(object? sender, EventArgs e)
    {
        if (_disposed) return;
        if (_state.CycleStartedAt.HasValue && !_state.CurrentPauseStartedAt.HasValue)
        {
            _state.CurrentPauseStartedAt = DateTime.Now;
            _state.CurrentPauseReason = PauseReason.AppShutdown;
        }
        _state.AppLastAliveAt = DateTime.Now;
        _state.SaveState();
    }

    private void ConfirmToggle()
    {
        var message = _loc.Get(IsStanding ? "Dialog_Swap_ToSitting" : "Dialog_Swap_ToStanding");
        _overlay.ShowConfirmation(_loc.Get("Dialog_Swap_Title"), message, result =>
        {
            if (!result) return;
            RecordCurrentCycle(CycleOutcome.Toggled);
            Toggle();
        });
    }

    private void ConfirmReset()
    {
        _overlay.ShowConfirmation(_loc.Get("Dialog_Reset_Title"), _loc.Get("Dialog_Reset_Message"), result =>
        {
            if (!result) return;
            RecordCurrentCycle(CycleOutcome.Reset);
            Reset();
        });
    }

    private DashboardWindow? _dashboard;

    private void ShowDashboard(int initialTabIndex = 0)
    {
        if (_dashboard != null)
        {
            if (_dashboard.DataContext is DashboardViewModel existingVm)
                existingVm.SelectedTabIndex = initialTabIndex;
            // Restore if the user minimized via taskbar / OS shortcut — without
            // this, Activate() on a minimized window can leave it in the tray
            // and the cog button appears to do nothing.
            if (_dashboard.WindowState == Avalonia.Controls.WindowState.Minimized)
                _dashboard.WindowState = Avalonia.Controls.WindowState.Normal;
            _dashboard.Activate();
            return;
        }

        var vm = new DashboardViewModel(_state, _history, RefreshOuterRing,
            onAlwaysOnTopChanged: () => OnPropertyChanged(nameof(AlwaysOnTop)),
            onClockFaceChanged: ReloadClockFace)
        {
            SelectedTabIndex = initialTabIndex
        };
        _dashboard = new DashboardWindow { DataContext = vm };
        _dashboard.Closed += (_, _) =>
        {
            // Dashboard subscribes to LocalizationService and UpdateService;
            // dispose so those handlers don't keep the (now-closed) view model
            // alive — open/close cycles would otherwise leak one VM per cycle.
            if (_dashboard?.DataContext is IDisposable disposable)
                disposable.Dispose();
            _dashboard = null;
        };

        var owner = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
        if (owner != null)
            _dashboard.Show(owner);
        else
            _dashboard.Show();
    }

    private void ShowTimeAdjustment()
    {
        _overlay.ShowTimeAdjustment(Time, result =>
        {
            if (result)
            {
                Time = _overlay.AdjustedTime;
                _currentCycleTimeEdited = true;
                ResetStopwatch();
            }
        });
    }

    private void ResetStopwatch()
    {
        // Keep the stopwatch's running-state in sync with IsRunning so that
        // elapsed time can't accumulate while we're paused (which would then
        // be subtracted from Time on the next Resume).
        if (IsRunning)
            _stopwatch.Restart();
        else
            _stopwatch.Reset();
    }

    private DateTime _lastPersistedAt = DateTime.MinValue;
    private static readonly TimeSpan PersistThrottle = TimeSpan.FromSeconds(5);

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var today = LogicalDay.From(DateTime.Now, _state.DayRolloverTime);
        if (today != _currentDay)
        {
            _currentDay = today;
            RefreshOuterRing();
            RefreshStreak();
        }

        // Day rollover is checked on every tick (cheap; usually a no-op) so
        // it fires while the app is open, not only on startup/unlock.
        ApplyDayRolloverIfDue();

        // Heartbeat: persist state at most every 5 s. On a crash/kill/forced
        // exit the recovery logic uses AppLastAliveAt to synthesize a pause
        // from "last alive" to next launch, so the worst we ever lose is one
        // throttle window.
        var now = DateTime.Now;
        if (now - _lastPersistedAt >= PersistThrottle)
        {
            _lastPersistedAt = now;
            _state.AppLastAliveAt = now;
            PersistCycleState();
        }

        if (!IsRunning) return;
        Time -= _stopwatch.Elapsed;
        _stopwatch.Restart();
        RefreshInnerRing();
        if (Time <= TimeSpan.Zero) _ = TriggerAsync();
    }

    private void OnScreenLocked(object? sender, EventArgs e)
    {
        // Only auto-pause if a cycle is actually running and we aren't already
        // in the middle of handling an end-of-cycle dialog.
        if (_triggerInFlight) return;
        if (!IsRunning) return;

        // Mode-conditional: only the current mode's "paused when away" setting
        // decides. Standing pauses by default, seated doesn't — see AppState.
        // When seated and not pausing, the timer keeps running while away, so
        // the next stand prompt may fire during the absence (intended).
        var pauseWhenAway = IsStanding ? _state.StandingPausedWhenAway : _state.SeatedPausedWhenAway;
        if (!pauseWhenAway) return;

        _autoPauseStartedAt = DateTime.Now;
        _autoPaused = true;
        Pause(_loc.Get("Mode_Locked"), PauseReason.ScreenLock);
    }

    private void OnScreenUnlocked(object? sender, EventArgs e)
    {
        if (!_autoPaused) return;
        _autoPaused = false;

        // If the user manually resumed in between (e.g. via another input
        // device that didn't fully unlock) skip the toast.
        if (IsRunning) return;

        var awayFor = DateTime.Now - _autoPauseStartedAt;
        Resume();

        // If the lock window straddled a rollover, the rollover toast wins —
        // it carries the more important "fresh day, mode reset" message.
        if (ApplyDayRolloverIfDue()) return;

        _overlay.ShowIdleResumed(awayFor);
    }
}
