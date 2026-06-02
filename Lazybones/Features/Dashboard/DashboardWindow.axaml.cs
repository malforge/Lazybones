using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Lazybones.Localization;

namespace Lazybones.Features.Dashboard;

public partial class DashboardWindow : Window
{
    public DashboardWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        (DataContext as DashboardViewModel)?.Dispose();
        base.OnClosed(e);
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    // File dialogs need the window's TopLevel, so the picker lives in
    // code-behind; the chosen path is handed to the view model, which persists
    // it and refreshes the disk.
    private async void ChooseClockFaceImage_OnClick(object? sender, RoutedEventArgs e)
    {
        var top = GetTopLevel(this);
        if (top is null) return;

        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = LocalizationService.Instance.Get("Settings_ClockFacePickerTitle"),
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll],
        });

        if (files.Count == 0) return;
        var path = files[0].TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
            (DataContext as DashboardViewModel)?.SetClockFaceImage(path);
    }
}
