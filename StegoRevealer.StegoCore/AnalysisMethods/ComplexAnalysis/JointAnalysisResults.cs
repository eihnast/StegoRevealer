using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;

namespace StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;

public class JointAnalysisResults
{
    public ChiSquareResult? ChiSquareResult { get; set; } = null;

    public RsResult? RsResult { get; set; } = null;

    public KzhaResult? KzhaResult { get; set; } = null;

    public StatmResult? StatmResult { get; set; } = null;

    public long ElapsedTime { get; set; } = 0;

    public bool? IsHidingDetected { get; set; } = null;

    public ComplexSaMethodResults? ComplexSaMethodResults { get; set; } = null;
}
