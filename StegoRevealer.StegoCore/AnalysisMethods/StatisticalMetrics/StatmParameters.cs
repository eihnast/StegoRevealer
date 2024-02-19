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


    // Параметры подсчёта шума

    /// <summary>Шаг выбора строк для подсчёта минимальных дисперсий (Метод 2)</summary>
    public int NoiseCalcSteps { get; set; } = 50;  // h

    /// <summary>Делитель высоты изображения, если с заданным шагом такого количества строк не выбрать (Метод 2)</summary>
    public int NoiseCalcStepsDivider { get; set; } = 8;

    /// <summary>Число интервалов, на которые разбивается строка (Метод 2)</summary>
    public int NoiseCalcIntervalNumber { get; set; } = 4;

    /// <summary>Число блоков по длинной стороне, на которые разбивается изображение (Метод 1)</summary>
    public int NoiseCalcBlocksNumber { get; set; } = 8;

    /// <summary>Число строк, которые фиксируются в блоке (Метод 1)</summary>
    public int NoiseCalcRowsInBlock { get; set; } = 3;


    public StatmParameters(ImageHandler image)
    {
        Image = image;
    }
}
