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

        var noiseCalcTask = new Task<double>(() => new NoiseCalculator(Params).CalcNoiseLevel(NoiseCalculator.NoiseCalculationMethod.Method2, true));  // Шум
        var sharpnessCalcTask = new Task<double>(() => new SharpnessCalculator(Params).CalcSharpness());  // Резкость

        noiseCalcTask.Start();
        sharpnessCalcTask.Start();
        noiseCalcTask.Wait();
        sharpnessCalcTask.Wait();

        result.NoiseValueMethod2 = noiseCalcTask.Result;
        result.SharpnessValue = sharpnessCalcTask.Result;

        result.Log($"Подсчёт характеристик завершён");
        return result;
    }
}
