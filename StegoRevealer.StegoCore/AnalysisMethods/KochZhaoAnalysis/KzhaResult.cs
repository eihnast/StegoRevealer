using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis
{
    public class KzhaResult : LoggedResult, IAnalysisResult
    {
        public LoggedResult AsLog()
        {
            return this;
        }

        // Результаты
        public bool SuspiciousIntervalIsFound { get; set; } = false;
        public double Threshold { get; set; }
        public ScIndexPair Coefficients { get; set; }
        public int MessageBitsVolume { get; set; }  // Относительный объём сообщения
        public string? ExtractedData { get; set; } = null;
        public (int leftInd, int rightInd)? SuspiciousInterval { get; set; }

        public KzhaResult()
        {
        }
    }
}
