using StegoRevealer.Common.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.CommandLine;

namespace StegoRevealer.Common;

public class TempManager
{
    // Описание синглтона
    private static TempManager? _instance;
    private static readonly object _lock = new object();
    public static TempManager Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                        _instance = new TempManager();
                }
            }
            return _instance;
        }
    }


    private TempManager()
    {
        // Приватный конструктор
    }


    private readonly List<TempFile> _tempImages = new List<TempFile>();
    private readonly List<TempFile> _tempFiles = new List<TempFile>();
    private readonly List<ImageHandler> _openedHandlers = new List<ImageHandler>();

    public void RememberTempImage(string originalPath, string tempPath) => _tempImages.Add(new TempFile { OriginalPath = originalPath, TempPath = tempPath });
    public void RememberTempFile(string originalPath, string tempPath) => _tempFiles.Add(new TempFile { OriginalPath = originalPath, TempPath = tempPath });

    public string? GetOriginalImageByTemp(string tempImgName) => 
        _tempImages.FirstOrDefault(img => Path.GetFileNameWithoutExtension(img.TempPath).Equals(tempImgName, StringComparison.OrdinalIgnoreCase))?.OriginalPath;
    public string? GetOriginalFileByTemp(string tempFileName) =>
        _tempFiles.FirstOrDefault(img => Path.GetFileNameWithoutExtension(img.TempPath).Equals(tempFileName, StringComparison.OrdinalIgnoreCase))?.OriginalPath;

    public void RememberHandler(ImageHandler imageHandler) => _openedHandlers.Add(imageHandler);
    public void ForgetHandler(ImageHandler imageHandler) => _openedHandlers.Remove(imageHandler);

    public void DeleteTempImages(bool withRetry = true, bool onlyWithoutHandlers = false, bool writeToLog = true)
    {
        var notDeleted = new List<string>();
        var processingImages = _tempImages.Where(img => File.Exists(img.TempPath) &&
                                                (!onlyWithoutHandlers || !_openedHandlers.Any(h => h.ImgPath.Equals(img.TempPath, StringComparison.OrdinalIgnoreCase))));

        foreach (var tempImage in processingImages)
        {
            try
            {
                File.Delete(tempImage.TempPath);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error wile deleting temp image: " + ex.Message);
                notDeleted.Add(tempImage.TempPath);
            }
        }

        if (withRetry && notDeleted.Count > 0)
        {
            Logger.LogWarning("Trying to retry deleting temp images");
            DeleteTempImages(false);
        }
        else
        {
            if (writeToLog)
                Logger.LogInfo("Temp image files deleted");
            _tempImages.Clear();
        }
    }

    public void DeleteImageHandlers()
    {
        foreach (var imageHandler in _openedHandlers)
        {
            try
            {
                imageHandler?.CloseHandler();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error wile closing opened image handler: " + ex.Message);
            }
        }

        Logger.LogInfo("All saved image handlers closed");
        _openedHandlers.Clear();
    }

    public void DeleteTempFiles(bool withRetry = true, bool writeToLog = true)
    {
        var notDeleted = new List<string>();
        var processingFiles = _tempImages.Where(img => File.Exists(img.TempPath));

        foreach (var tempFile in processingFiles)
        {
            try
            {
                File.Delete(tempFile.TempPath);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error wile deleting temp file: " + ex.Message);
                notDeleted.Add(tempFile.TempPath);
            }
        }

        if (withRetry && notDeleted.Count > 0)
        {
            Logger.LogWarning("Trying to retry deleting temp files");
            DeleteTempFiles(false);
        }
        else
        {
            if (writeToLog)
                Logger.LogInfo("Temp files deleted");
            _tempImages.Clear();
        }
    }
}
