using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.StegoMethods
{
    /// <summary>
    /// Результат стеганографического метода, содержащий логи
    /// </summary>
    public interface ILoggedStegoResult
    {
        /// <summary>
        /// Получить логи, записанные в результат стегоанализа
        /// </summary>
        public LoggedResult AsLog();
    }
}
