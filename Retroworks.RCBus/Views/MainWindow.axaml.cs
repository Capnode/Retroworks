using Avalonia.Controls;

namespace Retroworks.RCBus.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title += " " + System.IO.Directory.GetCurrentDirectory();
    }
}