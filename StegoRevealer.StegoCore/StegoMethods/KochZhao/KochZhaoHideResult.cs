using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    /// <summary>
    /// Результаты извлечения информации, скрытой по методу Коха-Жао
    /// </summary>
    public class KochZhaoHideResult : LoggedResult, IHideResult
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
