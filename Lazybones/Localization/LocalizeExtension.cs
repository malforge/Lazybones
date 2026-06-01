using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Lazybones.Localization;

// XAML markup extension: {l:Localize Settings_OpenAtLogin}.
//
// Binds to LocalizationService.CurrentLanguageCode (a plain string property
// that raises PropertyChanged every culture switch) and pipes the result
// through a converter that ignores the bound value and resolves Key against
// the service. Plain property bindings refresh reliably on INPC in Avalonia;
// the indexer-path shape (`[Key]`) does not, which is why this layer exists.
public sealed class LocalizeExtension : MarkupExtension
{
    private static readonly LookupConverter Converter = new();

    public LocalizeExtension() { }
    public LocalizeExtension(string key) => Key = key;

    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Source = LocalizationService.Instance,
            Path = nameof(LocalizationService.CurrentLanguageCode),
            Mode = BindingMode.OneWay,
            Converter = Converter,
            ConverterParameter = Key,
        };
    }

    private sealed class LookupConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => LocalizationService.Instance.Get(parameter as string ?? string.Empty);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
