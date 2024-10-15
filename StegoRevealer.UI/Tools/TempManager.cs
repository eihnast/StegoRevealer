using DynamicData;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System;
using System.Collections.Generic;
using System.IO;

namespace StegoRevealer.UI.Tools;

public class TempManager
{
    // Описание синглтона
    private static TempManager? _instance;
    private static object _lock = new object();
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


    private static List<string> _tempImages = new List<string>();
    private static List<ImageHandler> _openedHandlers = new List<ImageHandler>();

    public void RememberTempImage(string imagePath) => _tempImages.Add(imagePath);

    public void RememberHandler(ImageHandler imageHandler) => _openedHandlers.Add(imageHandler);
    public void ForgetHandler(ImageHandler imageHandler) => _openedHandlers.Remove(imageHandler);

    public void DeleteTempImages()
    {
        foreach (var imagePath in _tempImages)
        {
            if (File.Exists(imagePath))
            {
                try
                {
                    File.Delete(imagePath);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error wile deleting temp image: " + ex.Message);
                }
            }
        }

        _tempImages.Clear();
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

        _openedHandlers.Clear();
    }
}
