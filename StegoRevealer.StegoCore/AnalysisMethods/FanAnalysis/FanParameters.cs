using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;

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
    public double Threshold { get; set; } = 3.401714170610843;

    /* Порог вычислен на основе расстояний, вычисленных (методом CalcAdaptiveThreshold) на выборке FanComsTrainingSet (16000 "чистых" изображений)
     * k = 2.5: 5.4526680868742758
     * k = 2.0: 4.7690167814531312
     * k = 1.5: 4.0853654760319866
     * k = 1.0: 3.401714170610843
     * k = 0.9: 3.264983909526614
     * k = 0.8: 3.1282536484423851
     * k = 0.7: 2.9915233873581561
     * k = 0.6: 2.8547931262739272
     * k = 0.5: 2.7180628651896983
    */


    public FanParameters(ImageHandler image)
    {
        Image = image;
    }
}
