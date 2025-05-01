using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;

namespace StegoRevealer.StegoCore.CommonLib.Entities;

public class JointAnalysisMethodsParameters
{
    public ChiSquareParameters? ChiSquareParameters { get; set; } = null;

    public RsParameters? RsParameters { get; set; } = null;

    public SpaParameters? SpaParameters { get; set; } = null;

    public FanParameters? FanParameters { get; set; } = null;

    public ZcaParameters? ZcaParameters { get; set; } = null;

    public KzhaParameters? KzhaParameters { get; set; } = null;

    public ComplexSaMethodParameters? ComplexSaMethodParameters { get; set; } = null;

    public StatmParameters? StatmParameters { get; set; } = null;
}
