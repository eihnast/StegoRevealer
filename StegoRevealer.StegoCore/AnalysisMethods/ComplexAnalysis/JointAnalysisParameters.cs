using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;

public class JointAnalysisParameters
{
    public ImageHandler? Image { get; set; } = null;

    public ChiSquareParameters? ChiSquareParameters { get; set; } = null;

    public RsParameters? RsParameters { get; set; } = null;

    public KzhaParameters? KzhaParameters { get; set; } = null;

    public bool? UseComplexMethod { get; set; } = null;
}
