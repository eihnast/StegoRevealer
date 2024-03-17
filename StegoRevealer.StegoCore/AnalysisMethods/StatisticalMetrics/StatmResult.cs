using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;

/// <summary>
/// Результаты оценки статистических показателей
/// </summary>
public class StatmResult : LoggedResult, ILoggedAnalysisResult
{
    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }

    // Результаты

    /// <summary>
    /// Интенсивность шума по Методу 1
    /// </summary>
    public double NoiseValueMethod1 { get; set; }

    /// <summary>
    /// Интенсивность шума по Методу 2
    /// </summary>
    public double NoiseValueMethod2 { get; set; }

    /// <summary>
    /// Резкость изображения
    /// </summary>
    public double SharpnessValue { get; set; }

    /// <summary>
    /// Размытость изображения
    /// </summary>
    public double BlurValue { get; set; }

    public StatmResult() { }
}
