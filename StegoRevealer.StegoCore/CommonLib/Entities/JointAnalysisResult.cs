using StegoRevealer.StegoCore.AnalysisMethods;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.CommonLib.Entities;

public class JointAnalysisResult
{
    public ChiSquareResult? ChiSquareResult { get; set; } = null;

    public RsResult? RsResult { get; set; } = null;

    public SpaResult? SpaResult { get; set; } = null;

    public FanResult? FanResult { get; set; } = null;

    public ZcaResult? ZcaResult { get; set; } = null;

    public KzhaResult? KzhaResult { get; set; } = null;

    public StatmResult? StatmResult { get; set; } = null;

    public ComplexSaMethodResult? ComplexSaMethodResults { get; set; } = null;

    public long ElapsedTime { get; set; } = 0;


    public List<LogMessage> CollectErrors()
    {
        var errors = new List<LogMessage>();
        if (ChiSquareResult is not null)
            errors.AddRange(ChiSquareResult.AsLog().GetErrors());
        if (RsResult is not null)
            errors.AddRange(RsResult.AsLog().GetErrors());
        if (SpaResult is not null)
            errors.AddRange(SpaResult.AsLog().GetErrors());
        if (FanResult is not null)
            errors.AddRange(FanResult.AsLog().GetErrors());
        if (ZcaResult is not null)
            errors.AddRange(ZcaResult.AsLog().GetErrors());
        if (KzhaResult is not null)
            errors.AddRange(KzhaResult.AsLog().GetErrors());
        if (StatmResult is not null)
            errors.AddRange(StatmResult.AsLog().GetErrors());
        if (ComplexSaMethodResults is not null)
            errors.AddRange(ComplexSaMethodResults.AsLog().GetErrors());
        return errors;
    }
}
