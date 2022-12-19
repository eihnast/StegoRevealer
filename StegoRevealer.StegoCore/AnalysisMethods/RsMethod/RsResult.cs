using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod
{
    /// <summary>
    /// Результаты метода стегоанализа Regular-Singular
    /// </summary>
    public class RsResult : LoggedResult, ILoggedAnalysisResult
    {
        /// <inheritdoc/>
        public LoggedResult AsLog()
        {
            return this;
        }


        /// <summary>
        /// Относительный объём сообщения
        /// </summary>
        public double MessageRelativeVolume { get; set; }


        public RsResult() { }
    }
}
