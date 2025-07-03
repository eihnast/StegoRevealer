using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevelaer.API.Entities.RequestData;

public class StatmRequest : BaseAnalysisRequest
{

    public int NoiseCalcSteps { get; set; } = 50;
    public int NoiseCalcStepsDivider { get; set; } = 8;
    public int NoiseCalcIntervalNumber { get; set; } = 4;
    public int NoiseCalcBlocksNumber { get; set; } = 16;
    public int NoiseCalcFixedBlocksCount { get; set; } = 5;
    public int NoiseCalcRowsInBlock { get; set; } = 3;

    public byte SharpnessCalcWeakPixel { get; set; } = 25;
    public byte SharpnessCalcStrongPixel { get; set; } = 255;
    public int SharpnessCalcGuassianKernelSize { get; set; } = 5;
    public double SharpnessCalcGuassianKernelSigma { get; set; } = 1.0;
    public bool SharpnessCalcUseScharrOperator { get; set; } = false;
    public bool SharpnessCalcUseAveragedGrayscale { get; set; } = false;
    public int SharpnessCalcExtremumsNeighborhoodSize { get; set; } = 3;
    public double SharpnessCalcCannyUpThreshold { get; set; } = 0.5;
    public double SharpnessCalcCannyDownThreshold { get; set; } = 0.4;

    public int BlurCalcFilterSizeK1 { get; set; } = 5;
    public int BlurCalcFilterSizeK2 { get; set; } = 7;
    public bool BlurCalcUseAveragedGrayscale { get; set; } = false;

    public int ContrastCalcWindowCenterSize { get; set; } = 3;
    public bool ContrastCalcUseAveragedGrayscale { get; set; } = false;

    public EntropyMethods EntropyMethods { get; set; } = EntropyMethods.Shennon | EntropyMethods.Renyi;
    public double EntropyCalcSensitivity { get; set; } = 1.1;
    public bool EntropyCalcUseAveragedGrayscale { get; set; } = false;

    public StatmParameters CreateParameters(ImageHandler imgHandler)
    {
        var parameters = new StatmParameters(imgHandler)
        {
            NoiseCalcSteps = NoiseCalcSteps,
            NoiseCalcStepsDivider = NoiseCalcStepsDivider,
            NoiseCalcIntervalNumber = NoiseCalcIntervalNumber,
            NoiseCalcBlocksNumber = NoiseCalcBlocksNumber,
            NoiseCalcFixedBlocksCount = NoiseCalcFixedBlocksCount,
            NoiseCalcRowsInBlock = NoiseCalcRowsInBlock,

            SharpnessCalcWeakPixel = SharpnessCalcWeakPixel,
            SharpnessCalcStrongPixel = SharpnessCalcStrongPixel,
            SharpnessCalcGuassianKernelSize = SharpnessCalcGuassianKernelSize,
            SharpnessCalcGuassianKernelSigma = SharpnessCalcGuassianKernelSigma,
            SharpnessCalcCannyDownThreshold = SharpnessCalcCannyDownThreshold,
            SharpnessCalcCannyUpThreshold = SharpnessCalcCannyUpThreshold,
            SharpnessCalcExtremumsNeighborhoodSize = SharpnessCalcExtremumsNeighborhoodSize,
            SharpnessCalcUseAveragedGrayscale = SharpnessCalcUseAveragedGrayscale,
            SharpnessCalcUseScharrOperator = SharpnessCalcUseScharrOperator,
            
            BlurCalcFilterSizeK1 = BlurCalcFilterSizeK1,
            BlurCalcFilterSizeK2 = BlurCalcFilterSizeK2,
            BlurCalcUseAveragedGrayscale = BlurCalcUseAveragedGrayscale,

            ContrastCalcUseAveragedGrayscale = ContrastCalcUseAveragedGrayscale,
            ContrastCalcWindowCenterSize = ContrastCalcWindowCenterSize,

            EntropyCalcSensitivity = EntropyCalcSensitivity,
            EntropyCalcUseAveragedGrayscale = EntropyCalcUseAveragedGrayscale,
            EntropyMethods = EntropyMethods
        };

        return parameters;
    }
}
