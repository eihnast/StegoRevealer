using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.AnalysisMethods
{
    /// <summary>
    /// Результат стегоанализа, содержащий логи
    /// </summary>
    public interface ILoggedAnalysisResult
    {
        /// <summary>
        /// Получить логи, записанные в результат стегоанализа
        /// </summary>
        public LoggedResult AsLog();
    }
}
