namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;

public class SobelOperatorResult
{
    /// <summary>Матрица пикселей с выделением границ по оператору Собеля</summary>
    public byte[,] Intensity { get; set; } = new byte[0, 0];

    /// <summary>Матрица значений углов (в радиантах) направлений градиента для каждого пикселя</summary>
    public double[,] Direction { get; set; } = new double[0, 0];
}
