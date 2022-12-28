using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using System.Security.Principal;

namespace StegoRevealer.WinUi.Lib.Entities
{
    /// <summary>
    /// Данные результатов стегоанализа, передаваемые во View и для вывода
    /// </summary>
    public class SteganalysisResultsDto
    {
        public bool IsMethodChiSqrExecuted { get; private set; } = false;

        public double ChiSqrMessageRelativeVolume { get; private set; } = 0.0;

        public bool IsMethodRsExecuted { get; private set; } = false;

        public double RsMessageRelativeVolume { get; private set; } = 0.0;

        public bool IsMethodKzhaExecuted { get; private set; } = false;

        public bool KzhaSuspiciousIntervalIsFound { get; private set; } = false;

        public double KzhaThreshold { get; private set; } = 0.0;

        public ScIndexPair? KzhaCoefficients { get; private set; } = null;

        public int KzhaMessageBitsVolume { get; private set; } = 0;

        public string? KzhaExtractedData { get; private set; } = null;

        public (int leftInd, int rightInd)? KzhaSuspiciousInterval { get; private set; } = null;


        public long ElapsedTime { get; private set; } = 0;


        /// <summary>
        /// Заполняет DTO результатами стегоанализа<br/>
        /// Если результат по методу передан равным null, будет считаться, что метод не исполнялся
        /// </summary>
        public SteganalysisResultsDto(ChiSquareResult? chiRes = null, RsResult? rsRes = null, KzhaResult? kzhaRes = null,
            long? elapsedTime = null)
        {
            if (elapsedTime is not null)
                ElapsedTime = elapsedTime.Value;

            if (chiRes is not null)
            {
                IsMethodChiSqrExecuted = true;
                ChiSqrMessageRelativeVolume = chiRes.MessageRelativeVolume;
            }

            if (rsRes is not null)
            {
                IsMethodRsExecuted = true;
                RsMessageRelativeVolume = rsRes.MessageRelativeVolume;
            }

            if (kzhaRes is not null)
            {
                IsMethodKzhaExecuted = true;
                KzhaSuspiciousIntervalIsFound = kzhaRes.SuspiciousIntervalIsFound;
                KzhaThreshold = kzhaRes.Threshold;
                KzhaCoefficients = kzhaRes.Coefficients;
                KzhaMessageBitsVolume = kzhaRes.MessageBitsVolume;
                KzhaExtractedData = kzhaRes.ExtractedData;
                KzhaSuspiciousInterval = kzhaRes.SuspiciousInterval;
            }
        }
    }
}
