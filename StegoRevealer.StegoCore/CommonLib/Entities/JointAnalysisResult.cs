using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;

namespace StegoRevealer.StegoCore.CommonLib.Entities;

public class JointAnalysisResult
{
    public ChiSquareResult? ChiSquareResult { get; set; } = null;

    public RsResult? RsResult { get; set; } = null;

    public SpaResult? SpaResult { get; set; } = null;

    public ZcaResult? ZcaResult { get; set; } = null;

    public KzhaResult? KzhaResult { get; set; } = null;

    public StatmResult? StatmResult { get; set; } = null;

    public ComplexSaMethodResult? ComplexSaMethodResults { get; set; } = null;

    public long ElapsedTime { get; set; } = 0;
}
