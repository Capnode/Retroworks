using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using Retroworks.RCBus.ViewModels;

namespace Retroworks.RCBus.Views;

public partial class SettingsPageView : UserControl
{
    public SettingsPageView()
    {
        InitializeComponent();
        var services = (Application.Current as App)?.Services ?? throw new NullReferenceException(nameof(App)); 
        DataContext = services.GetRequiredService<SettingsPageViewModel>();
    }
}