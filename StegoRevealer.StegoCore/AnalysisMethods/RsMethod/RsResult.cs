using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod
{
    public class RsResult : LoggedResult, IAnalysisResult
    {
        public LoggedResult AsLog()
        {
            return this;
        }

        public double MessageRelativeVolume { get; set; }  // Относительный объём сообщения

        public RsResult()
        {
        }
    }
}
