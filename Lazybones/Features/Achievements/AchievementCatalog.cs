using System.Collections.Generic;
using Lazybones.Localization;

namespace Lazybones.Features.Achievements;

// Title and Description are resolved through LocalizationService on every
// access, so a language switch is reflected the next time the property is
// read — no string snapshotting at construction time.
public sealed record Achievement(string Id)
{
    public string Title => LocalizationService.Instance.Get($"Achievement_{Id}_Title");
    public string Description => LocalizationService.Instance.Get($"Achievement_{Id}_Description");
}

public static class AchievementCatalog
{
    public const string FirstStandId = "first_stand";
    public const string QuickDrawId = "quick_draw";
    public const string IronLegsId = "iron_legs";
    public const string EarlyBirdId = "early_bird";
    public const string NightOwlId = "night_owl";

    public const string WarmingUpId = "warming_up";
    public const string SevenDayStreakId = "seven_day_streak";
    public const string TwoWeekWonderId = "two_week_wonder";
    public const string HabitFormedId = "habit_formed";

    public const string DailyDriverId = "daily_driver";
    public const string OverachieverId = "overachiever";
    public const string DoubleDownId = "double_down";
    public const string PerfectDayId = "perfect_day";

    public const string CenturionId = "centurion";
    public const string LongHaulId = "long_haul";
    public const string MountaineerId = "mountaineer";

    public static IReadOnlyList<Achievement> All { get; } =
    [
        new(FirstStandId),
        new(QuickDrawId),
        new(IronLegsId),
        new(EarlyBirdId),
        new(NightOwlId),
        new(WarmingUpId),
        new(SevenDayStreakId),
        new(TwoWeekWonderId),
        new(HabitFormedId),
        new(DailyDriverId),
        new(OverachieverId),
        new(DoubleDownId),
        new(PerfectDayId),
        new(CenturionId),
        new(LongHaulId),
        new(MountaineerId),
    ];
}
