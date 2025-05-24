using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Retroworks.RCBus.Converters
{
    public class BooleanToColorBrushConverter : IValueConverter
    {
        public IBrush TrueBrush { get; set; } = Brushes.Lime; // Default brush for true
        public IBrush FalseBrush { get; set; } = Brushes.Blue; // Default brush for false

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? TrueBrush : FalseBrush;
            }

            return FalseBrush; // Default to FalseBrush if value is not a boolean
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // One-way binding only
        }
    }
}
