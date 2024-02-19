using Avalonia.Data.Converters;
using Avalonia.Data;
using System.Globalization;
using System;
using System.Collections.Generic;

namespace StegoRevealer.UI.Tools.MvvmTools;

public class WidthConverter : IValueConverter
{
    public static readonly WidthConverter Instance = new();
    public static Dictionary<string, double> DecreaseValues { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double sourceWidth)
        {
            double decreaseValue = 0;
            if (parameter is string decreaseName && DecreaseValues.ContainsKey(decreaseName))
                decreaseValue = DecreaseValues[decreaseName];
            return sourceWidth - decreaseValue;
        }
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
