using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using Retroworks.RCBus.ViewModels;

namespace Retroworks.RCBus.Views;

public partial class SerialPageView : UserControl
{
    public SerialPageView()
    {
        InitializeComponent();
        var services = (Application.Current as App)?.Services ?? throw new NullReferenceException(nameof(App));
        DataContext = services.GetRequiredService<SerialPageViewModel>();
    }
}