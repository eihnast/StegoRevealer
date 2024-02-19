using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;

/// <summary>
/// Анализатор статистических метрик изображения -
/// показателей визуального искажения
/// </summary>
public class StatmAnalyser
{
    private const string MethodName = "Statistical metricks calculator";

    /// <summary>
    /// Параметры метода
    /// </summary>
    public StatmParameters Params { get; set; }

    /// <summary>
    /// Внутренний метод-прослойка для записи в лог
    /// </summary>
    private Action<string> _writeToLog = (string str) => new string(str);


    public StatmAnalyser(ImageHandler image)
    {
        Params = new StatmParameters(image);
    }

    public StatmAnalyser(StatmParameters parameters)
    {
        Params = parameters;
    }


    /// <summary>
    /// Запуск анализа
    /// </summary>
    /// <param name="verboseLog">Вести подробный лог</param>
    public StatmResult Analyse(bool verboseLog = false)
    {
        var result = new StatmResult();
        result.Log($"Выполняется подсчёт характеристик для файла изображения {Params.Image.ImgName}");
        _writeToLog = result.Log;

        var noiseCalculator = new NoiseCalculator(Params);
        var noiseLevelM1 = noiseCalculator.CalcNoiseLevel(NoiseCalculator.NoiseCalculationMethod.Method1, true);
        result.NoiseValueMethod1 = noiseLevelM1;
        var noiseLevelM2 = noiseCalculator.CalcNoiseLevel(NoiseCalculator.NoiseCalculationMethod.Method2, true);
        result.NoiseValueMethod2 = noiseLevelM2;

        result.Log($"Подсчёт характеристик завершён");
        return result;
    }
}
