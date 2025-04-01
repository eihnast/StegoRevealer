namespace StegoRevealer.UI.Lib.ParamsHelpers;

/// <summary>
/// Базовый класс DTO параметров для методов стегоанализа
/// </summary>
public interface IParamsDto<T> where T : class
{
    /// <summary>
    /// Записывает значения из DTO в объект параметров
    /// </summary>
    public void FillParameters(ref T parameters);
}
