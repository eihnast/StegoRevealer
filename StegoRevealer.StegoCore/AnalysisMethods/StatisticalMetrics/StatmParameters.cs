using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics
{
    /// <summary>
    /// Параметры оценки статистических метрик
    /// </summary>
    public class StatmParameters
    {
        /// <summary>
        /// Изображение
        /// </summary>
        public ImageHandler Image { get; set; }

        // Остальные параметры
        public int NoiseCalcMethodSteps { get; set; } = 50;
        public int NoiseCalcMethodInterval { get; set; } = 4;

        public StatmParameters(ImageHandler image)
        {
            Image = image;
        }
    }
}
