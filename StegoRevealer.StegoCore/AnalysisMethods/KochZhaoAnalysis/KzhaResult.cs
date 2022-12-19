using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis
{
    /// <summary>
    /// Результаты стегоанализа метода Коха-Жао
    /// </summary>
    public class KzhaResult : LoggedResult, ILoggedAnalysisResult
    {
        /// <inheritdoc/>
        public LoggedResult AsLog()
        {
            return this;
        }


        /// <summary>
        /// Найден ли подозрительный интервал
        /// </summary>
        public bool SuspiciousIntervalIsFound { get; set; } = false;
        
        /// <summary>
        /// Вычисленный порог скрытия
        /// </summary>
        public double Threshold { get; set; }

        /// <summary>
        /// Коэффициенты, в которых найдено подозрение на скрытую информацию
        /// </summary>
        public ScIndexPair Coefficients { get; set; }

        /// <summary>
        /// Относительный объём сообщения
        /// </summary>
        public int MessageBitsVolume { get; set; }

        /// <summary>
        /// Извлечённая автоматически информация
        /// </summary>
        public string? ExtractedData { get; set; } = null;

        /// <summary>
        /// Граница (индексы блоков) подозрительного интервала
        /// </summary>
        public (int leftInd, int rightInd)? SuspiciousInterval { get; set; }


        public KzhaResult() { }
    }
}
