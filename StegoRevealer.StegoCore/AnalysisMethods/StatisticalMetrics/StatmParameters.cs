using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;

/// <summary>
/// Параметры оценки статистических метрик
/// </summary>
public class StatmParameters
{
    /// <summary>
    /// Изображение
    /// </summary>
    public ImageHandler Image { get; set; }


    // Остальные параметры

    /// <summary>
    /// Шаг выбора строк для подсчёта минимальных дисперсий
    /// </summary>
    public int NoiseCalcMethodSteps { get; set; } = 50;  // h

    /// <summary>
    /// Делитель высоты изображения, если с заданным шагом такого количества строк не выбрать
    /// </summary>
    public int NoiseCalcMethodStepsDivider { get; set; } = 8;

    /// <summary>
    /// 
    /// </summary>
    public int NoiseCalcMethodIntervalNumber { get; set; } = 4;


    public StatmParameters(ImageHandler image)
    {
        Image = image;
    }
}
