using Avalonia;
using Avalonia.Controls.Primitives;

namespace Retroworks.RCBus.Controls;

public partial class LedControl : TemplatedControl
{
    public static readonly StyledProperty<bool> IsOnProperty =
        AvaloniaProperty.Register<LedControl, bool>(nameof(IsOn), false);

    public bool IsOn
    {
        get => GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public static readonly StyledProperty<string> ColorProperty =
        AvaloniaProperty.Register<LedControl, string>(nameof(Color), "Lime");

    public string Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    static LedControl()
    {
        AffectsRender<LedControl>(IsOnProperty);
        AffectsRender<LedControl>(ColorProperty);
    }
}
