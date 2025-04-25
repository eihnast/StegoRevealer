using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.Fan;

/// <summary>
/// Параметры стегоанализа метода анализа аддитивного шума
/// </summary>
public class FanParameters
{
    /// <summary>
    /// Изображение
    /// </summary>
    public ImageHandler Image { get; set; }

    /// <summary>
    /// Пороговое значение 
    /// </summary>
    public double Threshold { get; set; } = 40;


    public FanParameters(ImageHandler image)
    {
        Image = image;
    }
}
