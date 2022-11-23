using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis
{
    public class KzhaParameters
    {
        public ImageHandler Image { get; set; }  // Изображение

        public UniqueList<ImgChannel> Channels { get; }  // Анализируемые каналы
            = new UniqueList<ImgChannel> { ImgChannel.Blue };

        // Минимальный порог разницы коэффициентов скрытия, превышение которого будет считаться сигналом о наличии скрытой информации
        public double Threshold { get; set; } = 20;

        // Порог отсечки для массива значений разности между dct-коэффициентами
        // (домножается на максимальное значение из последовательности C, что служит порогом определения подозрительной последовательности)
        public double CutCoefficient { get; set; } = 0.8;  // 0.2?

        // Анализируемые коэффициенты матрицы ДКП
        public List<ScIndexPair> AnalysisCoeffs { get; set; } = new()
        {
            HidingCoefficients.Coeff34,
            HidingCoefficients.Coeff35,
            HidingCoefficients.Coeff45
        };

        private const int BlockSize = 8;  // Линейный размер блока матрицы ДКП
        public int GetBlockSize() => BlockSize;

        public bool TryToExtract { get; set; } = true;  // Пробовать извлечь информацию автоматически

        public bool LoggingCSequences { get; set; } = false;

        public bool IsVerticalTraverse { get; set; } = false;  // Вертикальный обход матрицы блоков?


        public KzhaParameters(ImageHandler image)
        {
            Image = image;
        }
    }
}
