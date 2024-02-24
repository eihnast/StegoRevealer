namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;

public class CannyEdgeDetectionResult
{
    /// <summary>Матрица ЧБ пикселей с выделением границ (границы - не чёрные пиксели)</summary>
    public byte[,] EdgePixelsArray { get; set; } = new byte[0, 0];

    /// <summary>Оригинальная матрица ЧБ пикселей (после grayscaling-а изображения)</summary>
    public byte[,] GrayscaledPixelsArray { get; set; } = new byte[0, 0];

    /// <summary>Результаты выполнения метода Собеля для выделения границ и поиска направлений градиента</summary>
    public SobelOperatorResult SobelOperatorResult { get; set; } = new SobelOperatorResult();
}
