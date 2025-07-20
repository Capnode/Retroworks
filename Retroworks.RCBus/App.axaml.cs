using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Retroworks.Components;
using Retroworks.RCBus.Devices;
using Retroworks.RCBus.Services;
using Retroworks.RCBus.Services.Internals;
using Retroworks.RCBus.ViewModels;
using Retroworks.RCBus.Views;
using System.Linq;

[assembly: XmlnsDefinition("https://github.com/avaloniaui", "Retroworks.RCBus.Controls")]

namespace Retroworks.RCBus;

public partial class App : Application
{
    public ServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        var collection = new ServiceCollection();
        collection.AddSingleton<ISettings, Settings>();
        collection.AddSingleton<IConnection, SerialConnection>();
        collection.AddSingleton<IConnection, EmulatorConnection>();
        collection.AddSingleton<MainViewModel>();
        collection.AddSingleton<SerialPageViewModel>();
        collection.AddSingleton<EmulatorPageViewModel>();
        collection.AddSingleton<SettingsPageViewModel>();
        collection.AddTransient<IEmulatorDevice, SCM_F1>();
        collection.AddTransient<IEmulatorDevice, SCM_F2>();
        collection.AddTransient<IEmulatorDevice, SCM_S2>();
        collection.AddTransient<IEmulatorDevice, SCM_S3>();
        collection.AddTransient<IEmulatorDevice, SCM_S7>();
        collection.AddTransient<IEmulatorDevice, SCM_S8>();
        collection.AddSingleton<DialogService>();

        Services = collection.BuildServiceProvider();
        var settings = Services.GetRequiredService<ISettings>();
        var theme = settings.GetString("Theme");
        SetTheme(theme);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // If there are command line arguments, assume crash report
            var args = desktop.Args;
            if (args != null && args.Length >= 4)
            {
                desktop.MainWindow = ExceptionDialog(args[0], args[1], args[2], args[3]);
            }
            else
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Services.GetRequiredService<MainViewModel>()
                };
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView()
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void SetTheme(string? theme)
    {
        if (string.IsNullOrEmpty(theme)) return;
        switch (theme)
        {
            case nameof(ThemeVariant.Light):
                RequestedThemeVariant = ThemeVariant.Light;
                break;
            case nameof(ThemeVariant.Dark):
                RequestedThemeVariant = ThemeVariant.Dark;
                break;
            default:
                RequestedThemeVariant = ThemeVariant.Default;
                break;
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private Window ExceptionDialog(string title, string type, string message, string stack)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 640,
            Height = 480,
        };

        var button = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(10),
        };
        button.Click += (s, e) => dialog.Close();

        dialog.Content = new StackPanel
        {
            Children =
            {
                new TextBlock
                {
                    Text = type,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10)
                },
                new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10)
                },
                new TextBlock
                {
                    Text = stack,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10)
                },
                button
            }
        };

        return dialog;
    }
}