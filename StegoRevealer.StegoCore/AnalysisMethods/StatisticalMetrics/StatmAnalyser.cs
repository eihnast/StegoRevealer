using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Helpers;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using System.Diagnostics.CodeAnalysis;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics
{
    /// <summary>
    /// Анализатор статистических метрик изображения -
    /// показателей визуального искажения
    /// </summary>
    public class StatmAnalyser
    {
        private const string MethodName = "Statistical metricks calculator";

        /// <summary>
        /// Параметры метода
        /// </summary>
        public StatmParameters Params { get; set; }

        /// <summary>
        /// Внутренний метод-прослойка для записи в лог
        /// </summary>
        private Action<string> _writeToLog = (string str) => new string(str);


        public StatmAnalyser(ImageHandler image)
        {
            Params = new StatmParameters(image);
        }

        public StatmAnalyser(StatmParameters parameters)
        {
            Params = parameters;
        }


        /// <summary>
        /// Запуск анализа
        /// </summary>
        /// <param name="verboseLog">Вести подробный лог</param>
        public StatmResult Analyse(bool verboseLog = false)
        {
            var result = new StatmResult();
            result.Log($"Выполняется подсчёт характеристик для файла изображения {Params.Image.ImgName}");
            _writeToLog = result.Log;



            result.Log($"Подсчёт характеристик завершён");
            return result;
        }


        // Подсчёты уровня шума в изображении: "Метод 2"
        // https://cyberleninka.ru/article/n/metod-otsenki-urovnya-shuma-tsifrovogo-izobrazheniya/viewer

        private double[] mask5 = new double[5] { -3 / 35, 12 / 35, 17 / 35, 12 / 35, -3 / 35 };
        private double[] mask7 = new double[7] { -2 / 21, 3 / 21, 6 / 21, 7 / 21, 6 / 21, 3 / 21, -2 / 21 };

        private void CalcNoiseLevel()
        {
            var blocks = new ImageBlocks(new ImageBlocksParameters(Params.Image, Params.Image.Width, 1));
            var traverseOptions = new BlocksTraverseOptions()
            {
                TraverseType = CommonLib.TraverseType.Vertical,
                InterlaceChannels = false,
                Channels = new CommonLib.ScTypes.UniqueList<ImgChannel>() { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue },
                StartBlocks = new StegoMethods.StartValues((ImgChannel.Red, 0), (ImgChannel.Green, 0), (ImgChannel.Blue, 0))
            };
            var blocksIterator = BlocksTraverseHelper.GetForLinearAccessBlocks(blocks, traverseOptions);

            int k = 0;
            foreach (var block in blocksIterator)
            {
                if (k == Params.NoiseCalcMethodSteps)
                {


                    k = 0;
                }
                k++;
            }
        }

        private List<NoiseCalcMethodMinimumRowInfo> GetMinimumsRows(byte[,,] block)
        {
            if (block.GetLength(0) > 1)
                throw new Exception("Оценка шума прозводится только по строкам");

            var analysingChannels = new ImgChannel[3] { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };
            var intervals = new List<NoiseCalcMethodMinimumRowInfo>();
            int width = block.GetLength(1);
            int intervalLength = width / Params.NoiseCalcMethodInterval;

            foreach (var channel in analysingChannels)
            {
                
            }

            return intervals;
        }

        private double ApplyLinearOpertorA(int[] values, double[] mask)
        {
            if (values.Length != mask.Length)
                throw new ArgumentException("Длины отрезка значений и маски должны быть равны");

            double result = 0.0;
            for (int i = 0; i < mask.Length; i++)
                result += mask[i] * values[i];

            return result;
        }
    }
}
