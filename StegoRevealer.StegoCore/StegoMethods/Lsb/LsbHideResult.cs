using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.StegoMethods.Lsb
{
    /// <summary>
    /// Результаты извлечения информации из НЗБ
    /// </summary>
    public class LsbHideResult : LoggedResult, IHideResult
    {
        /// <summary>
        /// Путь к изображению со скрытой информацией
        /// </summary>
        public string? Path { get; set; } = null;

        /// <summary>
        /// Возвращает путь к изображению со скрытой информацией
        /// </summary>
        public string? GetResultPath() => Path;


        /// <inheritdoc/>
        public LoggedResult AsLog()
        {
            return this;
        }
    }
}
