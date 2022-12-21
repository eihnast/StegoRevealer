using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.WinUi.Lib.ParamsHelpers;
using System.Collections.Generic;

namespace StegoRevealer.WinUi.Lib.MethodsHelper
{
    /// <summary>
    /// DTO для параметров алгоритма стегоанализа метода Коха-Жао: 
    /// <see cref="KochZhaoParameters"/>
    /// </summary>
    public class KzhaParamsDto : BaseParamsDto<KzhaParameters>
    {
        public UniqueList<ImgChannel> Channels { get; set; } = new UniqueList<ImgChannel> { ImgChannel.Blue };

        public double Threshold { get; set; } = 20;

        public double CutCoefficient { get; set; } = 0.8;

        public List<ScIndexPair> AnalysisCoeffs { get; set; } = new()
        {
            HidingCoefficients.Coeff34,
            HidingCoefficients.Coeff35,
            HidingCoefficients.Coeff45
        };

        public bool TryToExtract { get; set; } = true;

        public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;


        public KzhaParamsDto() { }

        public KzhaParamsDto(KzhaParameters parameters)
        {
            Channels = new();
            foreach (var channel in parameters.Channels)
                Channels.Add(channel);

            Threshold = parameters.Threshold;
            CutCoefficient = parameters.CutCoefficient;

            AnalysisCoeffs = new();
            foreach (var coeff in parameters.AnalysisCoeffs)
                AnalysisCoeffs.Add(coeff);

            TryToExtract = parameters.TryToExtract;
            TraverseType = parameters.TraverseType;
        }

        /// <inheritdoc/>
        public override void FillParameters(ref KzhaParameters parameters)
        {
            if (parameters is null)
                return;

            parameters.Channels.Clear();
            foreach (var channel in Channels)
                parameters.Channels.Add(channel);

            parameters.Threshold = Threshold;
            parameters.CutCoefficient = CutCoefficient;

            parameters.AnalysisCoeffs = new();
            foreach (var coeff in AnalysisCoeffs)
                parameters.AnalysisCoeffs.Add(coeff);

            parameters.TryToExtract = TryToExtract;
            parameters.TraverseType = TraverseType;
        }
    }
}
