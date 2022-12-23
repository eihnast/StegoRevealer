namespace StegoRevealer.StegoCore.StegoMethods
{
    /// <summary>
    /// Класс реализации стеганографического извлечения
    /// </summary>
    public interface IExtractor
    {
        /// <summary>
        /// Запуск извлечения информации
        /// </summary>
        public IExtractResult Extract();

        /// <summary>
        /// Запуск извлечения информации со специфическими переданными параметрами
        /// </summary>
        /// <param name="parameters">Параметры</param>
        public IExtractResult Extract(IParams parameters);
    }
}
