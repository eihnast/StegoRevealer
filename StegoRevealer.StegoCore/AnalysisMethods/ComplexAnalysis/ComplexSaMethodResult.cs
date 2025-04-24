using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;

public class ComplexSaMethodResult : LoggedResult, ILoggedAnalysisResult
{
    /// <inheritdoc/>
    public LoggedResult AsLog()
    {
        return this;
    }


    public ChiSquareResult ChiSquareHorizontalResult { get; set; } = null!;
    public ChiSquareResult ChiSquareVerticalResult { get; set; } = null!;

    public RsResult RsResult { get; set; } = null!;

    public KzhaResult KzhaHorizontalResult { get; set; } = null!;
    public KzhaResult KzhaVerticalResult { get; set; } = null!;

    public StatmResult StatmResult { get; set; } = null!;

    public int PixelsNum { get; set; }

    public bool IsHidingDetected { get; set; }
    public double DecisionProbability { get; set; }

    /// <summary>
    /// Время, затраченное на анализ
    /// </summary>
    public long ElapsedTime { get; set; }


    public ComplexSaMethodResult() { }
}
