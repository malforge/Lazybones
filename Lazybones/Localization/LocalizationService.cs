using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Lazybones.Localization;

// Singleton language registry. Every user-facing string in the app routes
// through Get(key) or the LocalizeExtension XAML markup extension; random
// text pools route through Pick(poolName).
//
// Live switching: Apply() raises PropertyChanged("Item[]") so any binding
// to LocalizationService[key] refreshes without a window reopen. Non-XAML
// callers that cache a translation (e.g. derived properties on a ViewModel)
// must subscribe to CultureChanged and re-raise their own PropertyChanged.
public sealed class LocalizationService : INotifyPropertyChanged
{
    public const string AutoCode = "";

    // Order matters: the first entry is the canonical fallback used when a key
    // is missing from the active language's table, or when neither preference
    // nor OS culture maps to a supported language. Tables and Fallback are
    // declared BEFORE Instance so the singleton's instance-field initializers
    // (_current = Fallback) see them already populated — flipping that order
    // is a static-init NRE waiting to happen.
    private static readonly IReadOnlyList<LanguageTable> Tables =
    [
        new("en-US", "English", EnUs.Strings, EnUs.TextPools),
        new("nb-NO", "Norsk bokmål", NbNo.Strings, NbNo.TextPools),
    ];

    private static readonly LanguageTable Fallback = Tables[0];

    public static LocalizationService Instance { get; } = new();

    private LanguageTable _current = Fallback;
    private string _preference = AutoCode;

    private LocalizationService() { }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? CultureChanged;

    public string CurrentLanguageCode => _current.Code;

    // The user's stored preference (empty = follow OS). Distinct from
    // CurrentLanguageCode, which is what got resolved.
    public string Preference => _preference;

    public IReadOnlyList<(string Code, string DisplayName)> AvailableLanguages =>
        Tables.Select(t => (t.Code, t.DisplayName)).ToList();

    public void Apply(string? preference)
    {
        _preference = preference ?? AutoCode;
        var next = Resolve(_preference);
        if (next.Code == _current.Code) return;
        _current = next;
        // CurrentLanguageCode is what LocalizeExtension's converter binds to;
        // every {l:Localize Key} re-evaluates when this property changes.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguageCode)));
        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private static LanguageTable Resolve(string preference)
    {
        if (!string.IsNullOrEmpty(preference))
        {
            var match = Tables.FirstOrDefault(t =>
                string.Equals(t.Code, preference, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;
        }
        var os = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var byLang = Tables.FirstOrDefault(t =>
            new CultureInfo(t.Code).TwoLetterISOLanguageName == os);
        return byLang ?? Fallback;
    }

    public string this[string key] => Get(key);

    public string Get(string key)
    {
        if (_current.Strings.TryGetValue(key, out var s)) return s;
        if (Fallback.Strings.TryGetValue(key, out var fallback)) return fallback;
        return $"[{key}]";
    }

    public string Format(string key, params object?[] args) =>
        string.Format(new CultureInfo(_current.Code), Get(key), args);

    public string Pick(string poolName)
    {
        var pool = _current.TextPools.GetValueOrDefault(poolName)
                   ?? Fallback.TextPools.GetValueOrDefault(poolName);
        if (pool == null || pool.Length == 0) return $"[{poolName}]";
        return pool[Random.Shared.Next(pool.Length)];
    }

    private sealed record LanguageTable(
        string Code,
        string DisplayName,
        IReadOnlyDictionary<string, string> Strings,
        IReadOnlyDictionary<string, string[]> TextPools);
}
