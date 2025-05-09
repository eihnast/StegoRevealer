﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using StegoRevealer.StegoCore.AnalysisMethods;
using StegoRevealer.StegoCore.CommonLib.Exceptions;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.UI.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StegoRevealer.UI.Tools;

public static class CommonTools
{
    public static WriteableBitmap GetWriteableBitmapFromImageHandler(ImageHandler image)
    {
        var writeableBitmap = new WriteableBitmap(new PixelSize(image.Width, image.Height), new Vector(96.0, 96.0), PixelFormat.Bgra8888, null);

        var imgBytes = GetImageBytes(image);
        using (var frameBuffer = writeableBitmap.Lock())
        {
            Marshal.Copy(imgBytes, 0, frameBuffer.Address, imgBytes.Length);
        }

        return writeableBitmap;
    }

    public static Bitmap GetAvaloniaBitmapFromImageHandler(ImageHandler image)
    {
        var skBitmap = image.GetScImage().GetBitmap();
        if (skBitmap is null)
            throw new OperationException("Error while loading image");

        var bitmap = new Bitmap(
            PixelFormat.Bgra8888,
            AlphaFormat.Unpremul,
            skBitmap.GetPixels(),
            new PixelSize(skBitmap.Width, skBitmap.Height),
            new Vector(96.0, 96.0),
            skBitmap.RowBytes);
        return bitmap;
    }

    public static string ReplaceBadSymbols(string rawString, char symb = '�')
    {
        var symbolsArray = rawString.Select(c => IsBadSymbol(c) ? symb : c).ToArray();
        return new string(symbolsArray);
    }
    public static string FilterBadSymbols(string rawString)
    {
        var symbolsArray = rawString.Where(c => !IsBadSymbol(c) && !c.Equals('�')).ToArray();
        return new string(symbolsArray);
    }

    private static char[] AllowedSymbols = new char[] { '\r', '\n' };
    public static bool IsBadSymbol(char c)
    {
        if (AllowedSymbols.Contains(c))
            return false;
        return char.IsControl(c);
    }

    public static IEnumerable<string> SplitByLength(string str, int maxLength)
    {
        for (int index = 0; index < str.Length; index += maxLength)
        {
            yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
        }
    }

    public static byte[] GetImageBytes(string path)
    {
        var imageBytes = File.ReadAllBytes(path);
        return imageBytes;
    }

    public static byte[] GetImageBytes(ImageHandler image) => image.GetScImage().GetBitmap()?.Bytes ?? new byte[0];

    public static void TextInput_ValidationForInteger(object? sender, TextInputEventArgs e)
    {
        var tb = sender as TextBox;

        string newStr = e.Text ?? string.Empty;
        if (tb is not null)  // Проверяем целиком, иначе - только добавленное
            newStr = (tb.Text ?? string.Empty) + newStr;
        if (!int.TryParse(newStr, out int _))
            e.Handled = true;
    }

    public static void TextInput_ValidationForDouble(object? sender, TextInputEventArgs e)
    {
        var tb = sender as TextBox;

        string newStr = e.Text ?? string.Empty;
        if (tb is not null)  // Проверяем целиком, иначе - только добавленное
            newStr = (tb.Text ?? string.Empty) + newStr;
        if (!double.TryParse(newStr, out double _))
            e.Handled = true;
    }

    public static void PastingFromClipboardBlock(object? sender, RoutedEventArgs e) => e.Handled = true;

    public static SolidColorBrush GetBrush(string resourceName, SolidColorBrush? defaultBrush = null)
    {
        if (App.Current?.TryFindResource(resourceName, out var notSelectedBrush) ?? false)
        {
            var brush = notSelectedBrush as SolidColorBrush;
            if (brush is not null)
                return brush;
        }
        return defaultBrush ?? new SolidColorBrush();
    }

    public static T GetViewModel<T>(object? dataContext) where T : class
    {
        var viewModel = dataContext as T;
        if (viewModel is null)
        {
            StackFrame frame = new StackFrame(1);
            throw new OperationException($"Failed getting ViewModel '{typeof(T).Name}' for window/view '{frame.GetMethod()?.DeclaringType?.Name}'");
        }
        return viewModel;
    }

    public static void SetFields(string fieldName, bool value, params object[] objects)
    {
        foreach (var obj in objects)
            obj.GetType().GetProperty(fieldName)?.SetValue(obj, value);
    }

    /// <summary>
    /// Создаёт словарь с null-(default-)значениями указанного типа по методам стегоанализа
    /// </summary>
    public static Dictionary<AnalysisMethod, T?> CreateValuesByAnalysisMethodDictionary<T>()
    {
        var dict = new Dictionary<AnalysisMethod, T?>();
        foreach (AnalysisMethod method in Enum.GetValues(typeof(AnalysisMethod)))
            dict.Add(method, default);
        return dict;
    }

    private static List<Key> AllowedDigitKeys = new List<Key>() { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9 };
    private const Key DoubleSeparatorKey = Key.OemComma;
    private const Key MinusKey = Key.OemMinus;
    public static void FilterInput(TextBox textBox, KeyEventArgs e, FilterInputStrategy strategy)
    {
        if (strategy is FilterInputStrategy.AllowAll)
            return;

        if (strategy is FilterInputStrategy.AllowInteger or FilterInputStrategy.AllowDouble
            or FilterInputStrategy.AllowPositiveInteger or FilterInputStrategy.AllowPositiveDouble)
        {
            // Проверяем ввод числа
            if (!AllowedDigitKeys.Contains(e.Key))
            {
                // При вводе числа разрешён минус
                if (e.Key == MinusKey)
                {
                    if ((strategy is FilterInputStrategy.AllowInteger or FilterInputStrategy.AllowDouble)
                        && textBox.CaretIndex == 0 && (!textBox.Text?.Contains('-') ?? true))
                        return;
                }

                // При вводе числа разрешён ввод разделителя дробной части
                if (e.Key == DoubleSeparatorKey)
                {
                    if ((strategy is FilterInputStrategy.AllowDouble or FilterInputStrategy.AllowPositiveDouble)
                        && (textBox.CaretIndex > 0 && (!textBox.Text?.Contains(',') ?? true)))
                        return;
                }

                e.Handled = true;
            }
        }
    }
    public static void FilterInput(object? sender, KeyEventArgs e, FilterInputStrategy strategy)
    {
        var tb = sender as TextBox;
        if (tb is null)
            return;
        FilterInput(tb, e, strategy);
    }
}
