using SharpCompress.Common;
using SkiaSharp;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StegoRevealer.Common;

public static class Tools
{
    public static string GetFormattedJson<T>(T obj, bool notIndented = false)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = !notIndented,
            PropertyNamingPolicy = null,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,  // Убрать экранирование кириллицы в Unicode escaping
            Converters =
            {
                new JsonStringEnumConverter(),
                new LogMessageListConverter()
            },
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals  // Включает вывод NaN, Infinity
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
            string tempFileName = CreateTempFilenameWithoutExt(useGuid);
            string newPath = CreateTempPath(tempFileName + Path.GetExtension(filePath));

            try
            {
                File.Copy(filePath, newPath);
            }
            catch (Exception ex)
            {
                CommonLogger.LogError(ex.Message);
                return null;
            }

            return newPath;
        }

        return null;
    }

    public static string CreateTempFilenameWithoutExt(bool useGuid = false) => 
        useGuid ? Guid.NewGuid().ToString() : Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
    public static string CreateTempPath(string fileName) => Path.Combine(GetOrCreateTempDirPath(), fileName);

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
                    CommonLogger.LogError(ex.Message);
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    public static bool TrySaveImageFromBytes(byte[] imageData, string imgPath)
    {
        using var skStream = new SKMemoryStream(imageData);
        using var skBitmap = SKBitmap.Decode(skStream);

        if (skBitmap == null)
            return false;

        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var fileStream = File.OpenWrite(imgPath);
        data.SaveTo(fileStream);

        return true;
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

    private static List<string> _tabChangeMethods = ["TurnToAnalyzer", "TurnToHider", "TurnToExtractor", "TurnToInfoPage", "TurnToSettingsPage"];
    public static bool IsActionWhileTabChanged()
    {
        // Костыль, который проверяет, что вызов действия пришёл изначально из-за изменения вкладки.
        // Проблема в том, что при смене вкладки Avalonia почему-то вызывает метод свойства Unchecked на ToggleButton.
        // (вероятно, баг, т.к. для CheckBox-ов Unchecked не вызывается при смене вкладки) - из-за этого выбор метода/способа сбрасывается

        var stackTrace = new StackTrace();
        bool isTabChanged = stackTrace.GetFrames().FirstOrDefault(
            frame => StringContainsAnyOfSubstring(frame.GetMethod()?.Name ?? string.Empty, _tabChangeMethods)) is not null;
        return isTabChanged;
    }

    public static bool StringContainsAnyOfSubstring(string str, IEnumerable<string> substrings)
    {
        if (substrings.Any(substring => str.Contains(substring, StringComparison.OrdinalIgnoreCase)))
            return true;
        return false;
    }

    public static int GetContainerFrequencyVolume(ImageHandler img) => (img.Height / 8) * (img.Width / 8);

    public static string AddColon(string str, bool withSpace = true) => str + ":" + (withSpace ? " " : "");

    public static string GetElapsedTime(long elapsedTime) => elapsedTime.ToString() + " " + Constants.ResultsDefaults.ElapsedTimeMeasure;

    public static string GetValueAsPercents(double? value) => value is null ? $"{null}" : string.Format("{0:P2}", value);
    public static string GetFormattedDouble(double? value) => value is null ? $"{null}" : string.Format("{0:f2}", value);
    public static string GetLongFormattedDouble(double? value) => value is null or double.NaN ? $"–" : (value == 0.0 ? "0,0" : string.Format("{0:F5}", value));

    public static string GetStartOfBase64(string base64String) => base64String.Length > 25 ? base64String[..25] : base64String;

    public static bool IsLocalPath(string path) => new Uri(path).IsFile;
    public static bool IsWebPath(string path) => path.StartsWith(@"http://") || path.StartsWith(@"https://") || path.StartsWith(@"frp://");
}
