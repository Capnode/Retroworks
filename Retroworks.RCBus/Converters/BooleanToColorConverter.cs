using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Retroworks.RCBus.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public string TrueColor { get; set; } = "Green"; // Default color for true
        public string FalseColor { get; set; } = "DarkGray"; // Default color for false

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? TrueColor : FalseColor;
            }

            return FalseColor; // Default to FalseColor if value is not a boolean
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // One-way binding only
        }
    }
}
