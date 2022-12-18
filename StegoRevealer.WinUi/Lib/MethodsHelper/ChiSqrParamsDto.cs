using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.WinUi.Lib.ParamsHelpers;

namespace StegoRevealer.WinUi.Lib.MethodsHelper
{
    public class ChiSqrParamsDto : BaseParamsDto<ChiSquareParameters>
    {
        public bool Visualize { get; set; } = true;

        public bool IsVerticalTraverse { get; set; } = false;

        public bool ExcludeZeroPairs { get; set; } = true;

        public bool UseUnifiedCathegories { get; set; } = true;

        public int UnifyingCathegoriesThreshold { get; set; } = 4;

        public double Threshold { get; set; } = 0.95;

        public UniqueList<ImgChannel> Channels { get; set; }
            = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

        public int BlockWidth { get; set; } = 1;
        public int BlockHeight { get; set; } = 1;


        public int ImageWidth { get; init; }


        public ChiSqrParamsDto() { }

        public ChiSqrParamsDto(ChiSquareParameters parameters)
        {
            Visualize = parameters.Visualize;
            IsVerticalTraverse = parameters.IsVerticalTraverse;
            ExcludeZeroPairs = parameters.ExcludeZeroPairs;
            UseUnifiedCathegories = parameters.UseUnifiedCathegories;
            UnifyingCathegoriesThreshold = parameters.UnifyingCathegoriesThreshold;
            Threshold = parameters.Threshold;

            Channels = new();
            foreach (var channel in parameters.Channels)
                Channels.Add(channel);

            BlockWidth = parameters.BlockWidth;
            BlockHeight = parameters.BlockHeight;
        }

        public override void FillParameters(ref ChiSquareParameters parameters)
        {
            if (parameters is null)
                return;

            parameters.Visualize = Visualize;
            parameters.IsVerticalTraverse = IsVerticalTraverse;
            parameters.ExcludeZeroPairs = ExcludeZeroPairs;
            parameters.UseUnifiedCathegories = UseUnifiedCathegories;
            parameters.UnifyingCathegoriesThreshold = UnifyingCathegoriesThreshold;
            parameters.Threshold = Threshold;

            parameters.Channels.Clear();
            foreach (var channel in Channels)
                parameters.Channels.Add(channel);

            parameters.BlockWidth = BlockWidth;
            parameters.BlockHeight = BlockHeight;
        }
    }
}
