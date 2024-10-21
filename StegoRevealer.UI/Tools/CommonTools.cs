using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
            throw new Exception("Error while loading image");

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
            throw new Exception($"Failed getting ViewModel '{typeof(T).Name}' for window/view '{frame.GetMethod()?.DeclaringType?.Name}'");
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
    public static Dictionary<AnalyzerMethod, T?> CreateValuesByAnalyzerMethodDictionary<T>()
    {
        var dict = new Dictionary<AnalyzerMethod, T?>();
        foreach (AnalyzerMethod method in Enum.GetValues(typeof(AnalyzerMethod)))
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
            if (!AllowedDigitKeys.Contains(e.Key))
            {
                if (e.Key == MinusKey)
                {
                    if ((strategy is FilterInputStrategy.AllowInteger or FilterInputStrategy.AllowDouble)
                        && textBox.CaretIndex == 0 && (!textBox.Text?.Contains('-') ?? true))
                        return;
                }

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

    public static string GetFormattedJson<T>(T obj, bool notIndented = false)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = !notIndented,
            PropertyNamingPolicy = null,
            Converters =
            {
                new JsonStringEnumConverter(),
                new LogMessageListConverter()
            }
        });
    }

    private static string SavedTempPath = string.Empty;
    public static string GetOrCreateTempDirPath()
    {
        if (!string.IsNullOrEmpty(SavedTempPath))
        {
            if (!Path.Exists(SavedTempPath))
                Directory.CreateDirectory(SavedTempPath);
            return SavedTempPath;
        }

        string tempDir = Path.GetTempPath();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string? localDirPath = Directory.GetParent(tempDir)?.Parent?.FullName;
            if (!string.IsNullOrEmpty(localDirPath))
                tempDir = localDirPath;
        }

        var srTempPath = Path.Combine(tempDir, "StegoRevealer");
        if (!Path.Exists(srTempPath))
            Directory.CreateDirectory(srTempPath);
        tempDir = srTempPath;

        SavedTempPath = tempDir;
        return tempDir;
    }

    public static string? CopyFileToTemp(string filePath, bool useGuid = false)
    {
        if (File.Exists(filePath))
        {
            string tempFileName = useGuid ? Guid.NewGuid().ToString() : Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            string newPath = Path.Combine(GetOrCreateTempDirPath(), tempFileName + Path.GetExtension(filePath));

            try
            {
                File.Copy(filePath, newPath);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return null;
            }

            return newPath;
        }

        return null;
    }

    public static bool TryDeleteTempFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var dirName = Path.GetDirectoryName(filePath);
            var tempPath = GetOrCreateTempDirPath();

            if (IsPathsEquals(dirName, tempPath) || IsInFolder(dirName, tempPath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    public static bool IsPathsEquals(string? path1, string? path2)
    {
        if (path1 == path2)
            return true;

        if (!string.IsNullOrEmpty(path1) && !string.IsNullOrEmpty(path2))
        {
            return path1.Trim('/').Trim('\\').ToLower().Equals(path2.Trim('/').Trim('\\').ToLower());
        }

        return false;
    }

    public static bool IsInFolder(string? path, string? folderPath)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(folderPath))
            return false;

        string normalizedPath = path.Trim('/').Trim('\\').ToLower();
        string normalizedFolderPath = folderPath.Trim('/').Trim('\\').ToLower();
        return normalizedPath.StartsWith(normalizedFolderPath);
    }
}
