using Accord.IO;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.WinUi.Lib.ParamsHelpers;

namespace StegoRevealer.WinUi.Lib.MethodsHelper
{
    public class RsParamsDto : BaseParamsDto<RsParameters>
    {
        public UniqueList<ImgChannel> Channels { get; set; }
            = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

        public int PixelsGroupLength { get; set; } = 4;

        public int[] FlippingMask { get; set; } = new int[4] { 1, 0, 0, 1 };


        public RsParamsDto() { }

        public RsParamsDto(RsParameters parameters)
        {
            Channels = new();
            foreach (var channel in parameters.Channels)
                Channels.Add(channel);

            PixelsGroupLength = parameters.PixelsGroupLength;
            FlippingMask = (int[])parameters.FlippingMask.Clone();
        }

        public override void FillParameters(ref RsParameters parameters)
        {
            if (parameters is null)
                return;

            parameters.Channels.Clear();
            foreach (var channel in Channels)
                parameters.Channels.Add(channel);

            parameters.PixelsGroupLength = PixelsGroupLength;
            parameters.FlippingMask = (int[])FlippingMask.Clone();
        }
    }
}
