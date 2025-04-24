using StegoRevealer.StegoCore.CommonLib.Exceptions;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod;

/// <summary>
/// Вспомогательные функции и константы для метода RS
/// </summary>
public static class RsHelper
{
    // Константы и стандартные значения параметров метода RS

    /// <summary>
    /// Стандартное значение размера группы пикселей
    /// </summary>
    private const int _defaultPixelsGroupLength = 4;

    /// <summary>
    /// Стандартная длина групп пикселей
    /// </summary>
    public static int DefaultPixelsGroupLength { get; } = _defaultPixelsGroupLength;

    /// <summary>
    /// Стандартная маска флиппинга
    /// </summary>
    public static int[] DefaultFlippingMask { get; } = new int[_defaultPixelsGroupLength] { 1, 0, 0, 1 };

    /// <summary>
    /// Стандартная функция регулярности
    /// </summary>
    public static int DefaultRegularityFunction(IEnumerable<int> nums)
    {
        int sum = 0;
        for (int i = 0; i < nums.Count() - 1; i++)
            sum += Math.Abs(nums.ElementAt(i + 1) - nums.ElementAt(i));
        return sum;
    }

    /// <summary>
    /// Стандартная функция прямого флиппинга
    /// </summary>
    public static int DefaultFlipDirect(int value)
    {
        if ((value & 1) > 0)
            return value - 1;
        return value + 1;
    }

    /// <summary>
    /// Стандартная функция обратного флиппинга
    /// </summary>
    public static int DefaultFlipBack(int value)
    {
        if ((value & 1) > 0)
            return value + 1;
        return value - 1;
    }

    /// <summary>
    /// Стандартная функция нулевого флиппинга
    /// </summary>
    public static int DefaultFlipNone(int value) => value;


    // Вспомогательные методы реализации метода Regular-Singular

    /// <summary>
    /// Метод инвертирования маски
    /// </summary>
    public static int[] InvertMask(int[] mask) =>
        mask.Select(x => x * -1).ToArray();

    /// <summary>
    /// Метод получения стандартной инвертированной маски
    /// </summary>
    public static int[] GetDefaultInvertedMask() =>
        InvertMask(DefaultFlippingMask);

    /// <summary>
    /// Метод определения типа группы
    /// </summary>
    /// <param name="beforeFlippingResult">Значение регулярности до флиппинга</param>
    /// <param name="afterFlippingResult">Значение регулярности после флиппинга</param>
    public static RsGroupType DefineGroupType(int beforeFlippingResult, int afterFlippingResult)
    {
        if (afterFlippingResult > beforeFlippingResult)
            return RsGroupType.Regular;
        if (afterFlippingResult < beforeFlippingResult)
            return RsGroupType.Singular;
        if (afterFlippingResult == beforeFlippingResult)
            return RsGroupType.Unusuable;

        throw new CalculationException("Error while processing group type definition");
    }
    public static RsGroupType DefineGroupType((int beforeFlippingResult, int afterFlippingResult) regularityResult) =>
        DefineGroupType(regularityResult.beforeFlippingResult, regularityResult.afterFlippingResult);

    /// <summary>
    /// Применить функции флиппинга к группе
    /// </summary>
    public static int[] ApplyFlipping(IEnumerable<int> group, Func<int, int>[] funcs)
    {
        if (group.Count() != funcs.Length)
            throw new ArgumentException("Group length and number of flipping functions should be equal");

        var flippedGroup = new int[group.Count()];
        for (int i = 0; i < group.Count(); i++)
            flippedGroup[i] = funcs[i](group.ElementAt(i));
        return flippedGroup;
    }
}
