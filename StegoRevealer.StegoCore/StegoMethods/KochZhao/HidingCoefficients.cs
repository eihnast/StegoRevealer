using StegoRevealer.StegoCore.CommonLib.ScTypes;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao;

/// <summary>
/// Наборы индексов коэффициентов для блоков матрицы пикселей и блоков ДКП<br/>
/// Коэффициенты в формате (y, x)
/// </summary>
public static class HidingCoefficients
{
    public static readonly ScIndexPair Coeff12 = new ScIndexPair(0, 1);
    public static readonly ScIndexPair Coeff13 = new ScIndexPair(0, 2);
    public static readonly ScIndexPair Coeff14 = new ScIndexPair(0, 3);
    public static readonly ScIndexPair Coeff15 = new ScIndexPair(0, 4);
    public static readonly ScIndexPair Coeff16 = new ScIndexPair(0, 5);
    public static readonly ScIndexPair Coeff17 = new ScIndexPair(0, 6);
    public static readonly ScIndexPair Coeff18 = new ScIndexPair(0, 7);
    public static readonly ScIndexPair Coeff23 = new ScIndexPair(1, 2);
    public static readonly ScIndexPair Coeff24 = new ScIndexPair(1, 3);
    public static readonly ScIndexPair Coeff25 = new ScIndexPair(1, 4);
    public static readonly ScIndexPair Coeff26 = new ScIndexPair(1, 5);
    public static readonly ScIndexPair Coeff27 = new ScIndexPair(1, 6);
    public static readonly ScIndexPair Coeff28 = new ScIndexPair(1, 7);
    public static readonly ScIndexPair Coeff34 = new ScIndexPair(2, 3);
    public static readonly ScIndexPair Coeff35 = new ScIndexPair(2, 4);
    public static readonly ScIndexPair Coeff36 = new ScIndexPair(2, 5);
    public static readonly ScIndexPair Coeff37 = new ScIndexPair(2, 6);
    public static readonly ScIndexPair Coeff38 = new ScIndexPair(2, 7);
    public static readonly ScIndexPair Coeff45 = new ScIndexPair(3, 4);
    public static readonly ScIndexPair Coeff46 = new ScIndexPair(3, 5);
    public static readonly ScIndexPair Coeff47 = new ScIndexPair(3, 6);
    public static readonly ScIndexPair Coeff48 = new ScIndexPair(3, 7);
    public static readonly ScIndexPair Coeff56 = new ScIndexPair(4, 5);
    public static readonly ScIndexPair Coeff57 = new ScIndexPair(4, 6);
    public static readonly ScIndexPair Coeff58 = new ScIndexPair(4, 7);
    public static readonly ScIndexPair Coeff67 = new ScIndexPair(5, 6);
    public static readonly ScIndexPair Coeff68 = new ScIndexPair(5, 7);
    public static readonly ScIndexPair Coeff78 = new ScIndexPair(6, 7);
}
