using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;
using Retroworks.RCBus.Services;
using Retroworks.RCBus.ViewModels;
using Retroworks.RCBus.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using Retroworks.RCBus.Services.Internals;
using Avalonia.Data.Core.Plugins;
using Retroworks.RCBus.Devices;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.Styling;

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
        // Register global exception handlers
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

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
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
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

    private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // Log the exception or handle it as needed
        await ShowExceptionDialog(e.ExceptionObject as Exception);

        // Note: The application will still terminate if the exception is critical.
        // You cannot fully suppress termination for certain exceptions (e.g., OutOfMemoryException).
    }

    private async void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved(); // Prevent the process from terminating
        await ShowExceptionDialog(e.Exception);
    }

    private async Task ShowExceptionDialog(Exception? exception)
    {
        if (exception == null) return;

        var dialog = new Window
        {
            Title = "Unhandled Exception",
            Width = 400,
            Height = 200,
        };
        dialog.Content = new StackPanel
        {
            Children =
            {
                new TextBlock
                {
                    Text = $"An unhandled exception occurred:\n{exception.Message}",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10)
                },
                new Button
                {
                    Content = "OK",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Thickness(10),
                    Command = ReactiveCommand.Create(() => dialog.Close())
                }
            }
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            await dialog.ShowDialog(desktop.MainWindow); // Block until the dialog is closed
        }
    }
}