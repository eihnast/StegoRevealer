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
using System.Runtime.InteropServices;

namespace StegoRevealer.UI.Tools
{
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
    }
}
