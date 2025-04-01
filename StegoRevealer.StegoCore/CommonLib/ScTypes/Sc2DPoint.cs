namespace StegoRevealer.StegoCore.CommonLib.ScTypes;

/// <summary>
/// Координаты 2D-точки
/// </summary>
public struct Sc2DPoint : IScValuesPair<int>
{
    /// <summary>
    /// Y-координата (значение на оси ординат) - первое значение
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// X-координата (значение на оси абсцисс) - второе значение
    /// </summary>
    public int X { get; set; }


    /// <inheritdoc/>
    public int FirstValue { get { return Y; } set { Y = value; } }
    /// <inheritdoc/>
    public int SecondValue { get { return X; } set { X = value; } }


    public Sc2DPoint(int y, int x)
    {
        Y = y;
        X = x;
    }

    /// <summary>
    /// Преобразовать координаты точки в пару индексов
    /// </summary>
    public ScIndexPair AsPair() => new ScIndexPair(Y, X);

    /// <summary>
    /// Получить значения координат точки как кортеж
    /// </summary>
    public (int, int) AsTuple() => (Y, X);
}
