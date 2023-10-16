using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;

namespace StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis
{
    /// <summary>
    /// Параметры стегоанализа метода Коха-Жао
    /// </summary>
    public class KzhaParameters
    {
        /// <summary>
        /// Изображение
        /// </summary>
        public ImageHandler Image { get; set; }

        /// <summary>
        /// Анализируемые каналы
        /// </summary>
        public UniqueList<ImgChannel> Channels { get; }
            = new UniqueList<ImgChannel> { ImgChannel.Blue };

        /// <summary>
        /// Минимальный порог разницы коэффициентов скрытия, превышение которого будет считаться сигналом о наличии скрытой информации
        /// </summary>
        public double Threshold { get; set; } = 20;

        /// <summary>
        /// Порог отсечки для массива значений разности между dct-коэффициентами<br/>
        /// (домножается на максимальное значение из последовательности C, что служит порогом определения подозрительной последовательности)
        /// </summary>
        public double CutCoefficient { get; set; } = 0.65;  // 0.2?

        /// <summary>
        /// Анализируемые коэффициенты матрицы ДКП
        /// </summary>
        public List<ScIndexPair> AnalysisCoeffs { get; set; } = new()
        {
            HidingCoefficients.Coeff34,
            HidingCoefficients.Coeff35,
            HidingCoefficients.Coeff45
        };

        /// <summary>
        /// Возвращает линейный размер блока матрицы ДКП
        /// </summary>
        public int BlockSize { get; } = 8;

        /// <summary>
        /// Пробовать извлечь информацию автоматически
        /// </summary>
        public bool TryToExtract { get; set; } = true;

        /// <summary>
        /// Включить логирование полных последовательностей cSequence
        /// </summary>
        public bool LoggingCSequences { get; set; } = false;

        /// <summary>
        /// Тип обхода матрицы блоков
        /// </summary>
        public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;


        public KzhaParameters(ImageHandler image)
        {
            Image = image;
        }
    }
}
