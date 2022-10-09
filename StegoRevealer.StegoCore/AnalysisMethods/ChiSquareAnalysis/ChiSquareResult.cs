using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis
{
    public class ChiSquareResult : LoggedResult, IAnalysisResult
    {
        public LoggedResult AsLog()
        {
            return this;
        }

        public int MessageLength { get; set;  }  // Размер скрытого сообщения

        public double MessageRelativeVolume { get; set; }  // Относительный объём сообщения


        public ImageHandler? Image { get; set; }  // Изображение (если визуализация включена)


        public ChiSquareResult()
        {
        }
    }
}
