using StegoRevealer.StegoCore.CommonLib.ScTypes;

namespace StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis
{
    /// <summary>
    /// Промежуточные результаты выполнения стегоанализа метода Коха-Жао для одной пары коэффициентов
    /// </summary>
    public struct OneCoeffsPairAnalysisResult
    {
        /// <summary>
        /// Обнаруженный порог
        /// </summary>
        public double Threshold { get; set; }

        /// <summary>
        /// Предполагаемые индексы подозрительного интервала, если он найден
        /// </summary>
        public ScIndexPair? Indexes { get; set; }

        /// <summary>
        /// Найден ли подозрительный интервал
        /// </summary>
        public bool HasSuspiciousInterval { get; set; }


        public OneCoeffsPairAnalysisResult(double threshold, ScIndexPair? indexes, bool hasSuspiciousInterval)
        {
            Threshold = threshold;
            Indexes = indexes;
            HasSuspiciousInterval = hasSuspiciousInterval;
        }
    }
}
