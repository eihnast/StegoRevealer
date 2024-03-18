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

        var noiseCalcTask = new Task<double>(() => new NoiseCalculator(Params).CalcNoiseLevel(NoiseCalculator.NoiseCalculationMethod.Method2));  // Шум
        var sharpnessCalcTask = new Task<double>(() => new SharpnessCalculator(Params).CalcSharpness());  // Резкость
        var blurCalcTask = new Task<double>(() => new BlurCalculator(Params).CalcBlur());  // Размытость
        var contrastCalcTask = new Task<double>(() => new ContrastCalculator(Params).CalcContrast());  // Контраст
        var entropyTsallisCalcTask = new Task<double>(() => new EntropyCalculator(Params).CalcTsallisEntropy());  // Контраст - Tsallis
        var entropyVaidaCalcTask = new Task<double>(() => new EntropyCalculator(Params).CalcVaidaEntropy());  // Контраст - Vaida

        noiseCalcTask.Start();
        sharpnessCalcTask.Start();
        blurCalcTask.Start();
        contrastCalcTask.Start();
        entropyTsallisCalcTask.Start();
        entropyVaidaCalcTask.Start();

        noiseCalcTask.Wait();
        sharpnessCalcTask.Wait();
        blurCalcTask.Wait();
        contrastCalcTask.Wait();
        entropyTsallisCalcTask.Wait();
        entropyVaidaCalcTask.Wait();

        result.NoiseValueMethod2 = noiseCalcTask.Result;
        result.SharpnessValue = sharpnessCalcTask.Result;
        result.BlurValue = blurCalcTask.Result;
        result.ContrastValue = contrastCalcTask.Result;
        result.EntropyTsallisValue = entropyTsallisCalcTask.Result;
        result.EntropyVaidaValue = entropyVaidaCalcTask.Result;

        result.Log($"Подсчёт характеристик завершён");
        return result;
    }
}
