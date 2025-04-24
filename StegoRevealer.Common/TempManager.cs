using StegoRevealer.Common.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;

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


    private readonly List<TempImage> _tempImages = new List<TempImage>();
    private readonly List<ImageHandler> _openedHandlers = new List<ImageHandler>();

    public void RememberTempImage(string originalPath, string tempPath) => _tempImages.Add(new TempImage { OriginalPath = originalPath, TempPath = tempPath });

    public string? GetOriginalImageByTemp(string tempImgName) => 
        _tempImages.FirstOrDefault(img => Path.GetFileNameWithoutExtension(img.TempPath).Equals(tempImgName, StringComparison.OrdinalIgnoreCase))?.OriginalPath;

    public void RememberHandler(ImageHandler imageHandler) => _openedHandlers.Add(imageHandler);
    public void ForgetHandler(ImageHandler imageHandler) => _openedHandlers.Remove(imageHandler);

    public void DeleteTempImages(bool withRetry = true)
    {
        var notDeleted = new List<string>();
        
        foreach (var tempImage in _tempImages.Where(img => File.Exists(img.TempPath)))
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
                Logger.LogInfo("All saved image handlers closed");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error wile closing opened image handler: " + ex.Message);
            }
        }

        _openedHandlers.Clear();
    }
}
