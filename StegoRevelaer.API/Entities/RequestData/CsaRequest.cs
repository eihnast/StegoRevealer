using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevelaer.API.Entities.RequestData;

public class CsaRequest : BaseAnalysisRequest
{
    public bool Visualize { get; set; } = false;
    public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;
    public bool UseSeparateChannelsCalc { get; set; } = true;
    public bool UseUnitedCnum { get; set; } = true;
    public bool UsePreviousCnums { get; set; } = true;
    public bool ExcludeZeroPairs { get; set; } = true;
    public bool UseUnifiedCathegories { get; set; } = true;
    public int UnifyingCathegoriesThreshold { get; set; } = 4;
    public double Threshold { get; set; } = 0.95;
    public ImgChannel[] Channels { get; set; }
        = new ImgChannel[] { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };
    public int? BlockWidth { get; set; }
    public int? BlockHeight { get; set; }

    public ChiSquareParameters CreateParameters(ImageHandler imgHandler)
    {
        var parameters = new ChiSquareParameters(imgHandler)
        {
            Visualize = Visualize,
            TraverseType = TraverseType,
            UseSeparateChannelsCalc = UseSeparateChannelsCalc,
            UseUnitedCnum = UseUnitedCnum,
            UsePreviousCnums = UsePreviousCnums,
            ExcludeZeroPairs = ExcludeZeroPairs,
            UseUnifiedCathegories = UseUnifiedCathegories,
            UnifyingCathegoriesThreshold = UnifyingCathegoriesThreshold,
            Threshold = Threshold
        };

        parameters.Channels.Clear();
        parameters.Channels.AddRange(Channels);

        parameters.BlockWidth = BlockWidth ?? imgHandler.Width;
        parameters.BlockHeight = BlockHeight ?? 1;

        return parameters;
    }
}
