using Retroworks.RCBus.Services.Internals;
using System.Reflection;
using Retroworks.RCBus.Services;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using CommunityToolkit.Mvvm.Input;

namespace Retroworks.RCBus.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase
{
    public static ThemeVariant[] Themes => new ThemeVariant[] { ThemeVariant.Light, ThemeVariant.Dark };

    private readonly ISettings _settings;

    /// <summary>
    /// Design-time only constructor
    /// Allow nullable PageFactory for now in designer... ideally get it working
    /// </summary>
#pragma warning disable CS8618, CS9264
    public SettingsPageViewModel()
    {
    }
#pragma warning restore CS8618, CS9264

    public SettingsPageViewModel(MainViewModel mainViewModel, DialogService dialogService, ISettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        if (Application.Current is not Application app) throw new ApplicationException(nameof(Application.Current)); ;
        _actualTheme = app.ActualThemeVariant;
    }

    public static string Title => Assembly.GetExecutingAssembly().GetName().Name ?? "Unknown";
    public static string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    public static string? Copyright => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
    public static string? Author => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
    public static string? Description => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

    [RelayCommand]
    private void Reset()
    {
        if (_settings == null) throw new NullReferenceException(nameof(_settings));
        _settings.Reset();
        ActualTheme = ThemeVariant.Default;
    }

    [ObservableProperty]
    public ThemeVariant _actualTheme;
    partial void OnActualThemeChanged(ThemeVariant value)
    {
        if (value == default) return;
        if (Application.Current is not Application app) throw new ApplicationException(nameof(Application.Current));
        if (app.ActualThemeVariant == value) return;
        app.RequestedThemeVariant = value;
        _settings.SetString("Theme", value.ToString());
    }
}