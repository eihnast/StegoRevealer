namespace StegoRevealer.StegoCore.StegoMethods;

/// <summary>
/// Класс реализации стеганографического скрытия
/// </summary>
public interface IHider
{
    /// <summary>
    /// Запуск скрытия информации
    /// </summary>
    /// <param name="data">Скрываемая информация</param>
    public IHideResult Hide(string? data, string? newImagePath = null);

    /// <summary>
    /// Запуск скрытия информации со специфическими указанными параметрами
    /// </summary>
    /// <param name="parameters">Параметры</param>
    /// <param name="data">Скрываемая информация</param>
    public IHideResult Hide(IParams parameters, string? data, string? newImagePath = null);
}
