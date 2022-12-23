namespace StegoRevealer.StegoCore.StegoMethods
{
    /// <summary>
    /// Класс реализации стеганографического скрытия
    /// </summary>
    public interface IHider
    {
        /// <summary>
        /// Запуск скрытия информации
        /// </summary>
        /// <param name="data">Скрываемая информация</param>
        public IHideResult Hide(string? data);

        // TODO: Добавить метод Hide с указанием IParams (и data) - аналогично как в IExtractor
    }
}
