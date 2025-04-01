namespace StegoRevealer.StegoCore.CommonLib.ScTypes;

/// <summary>
/// Базовое представление пары значений
/// </summary>
public interface IScValuesPair<T>
{
    /// <summary>
    /// Первое значение
    /// </summary>
    public T FirstValue { get; set; }

    /// <summary>
    /// Второе значение
    /// </summary>
    public T SecondValue { get; set; }


    /// <summary>
    /// Получить значения пары как кортеж
    /// </summary>
    public (T firstValue, T secondValue) AsTuple();
}
