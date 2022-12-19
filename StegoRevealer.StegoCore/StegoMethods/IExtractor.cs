namespace StegoRevealer.StegoCore.StegoMethods
{
    /// <summary>
    /// Класс реализации стеганографического извлечения
    /// </summary>
    public interface IExtractor
    {
        /// <summary>
        /// Запуск извлечения информации с указанными параметрами
        /// </summary>
        /// <param name="parameters">Параметры</param>
        public IExtractResult Extract(IParams parameters);

        /// <summary>
        /// Запуск извлечения информации со стандартными параметрами
        /// </summary>
        public IExtractResult Extract();
    }
}
