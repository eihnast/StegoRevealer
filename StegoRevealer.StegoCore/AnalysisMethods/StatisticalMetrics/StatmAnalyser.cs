using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Helpers;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Channels;

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

            var noiseLevel = CalcNoiseLevel();
            result.NoiseValue = noiseLevel;

            result.Log($"Подсчёт характеристик завершён");
            return result;
        }


        // Подсчёты уровня шума в изображении: "Метод 2"
        // https://cyberleninka.ru/article/n/metod-otsenki-urovnya-shuma-tsifrovogo-izobrazheniya/viewer

        private double[] mask5 = new double[5] { -3 / 35, 12 / 35, 17 / 35, 12 / 35, -3 / 35 };
        private double[] mask7 = new double[7] { -2 / 21, 3 / 21, 6 / 21, 7 / 21, 6 / 21, 3 / 21, -2 / 21 };

        private double CalcNoiseLevel()
        {
            var blocks = new ImageBlocks(new ImageBlocksParameters(Params.Image, Params.Image.Width, 1));

            // Если недостаточно высоты изображения для выбора строк с заданным шагом (по умолчанию - 50)
            if (blocks.BlocksInColumn < Params.NoiseCalcMethodSteps * Params.NoiseCalcMethodStepsDivider)
                Params.NoiseCalcMethodSteps = blocks.BlocksInColumn / Params.NoiseCalcMethodStepsDivider;

            var traverseOptions = new BlocksTraverseOptions()
            {
                TraverseType = CommonLib.TraverseType.Vertical,
                Channels = new CommonLib.ScTypes.UniqueList<ImgChannel>() { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue }
            };
            var blocksIterator = BlocksTraverseHelper.GetForLinearAccessBlocks(blocks, traverseOptions);

            var rowsMinDispersions = new Dictionary<int, double>();
            var allIntervals = new List<NoiseCalcMethodIntervalsInRowInfo>();

            int k = 1;
            foreach (var block in blocksIterator)
            {
                if (k % Params.NoiseCalcMethodSteps == 0)
                {
                    int rowId = k - 1;
                    var intervalsInfo = GetIntervalsInRow(block);
                    double minDispersion = intervalsInfo[0].Dispersion;

                    foreach (var interval in intervalsInfo)
                    {
                        if (interval.Dispersion < minDispersion)
                            minDispersion = interval.Dispersion;
                        interval.RowId = rowId;
                    }

                    rowsMinDispersions.Add(rowId, minDispersion);
                    allIntervals.AddRange(intervalsInfo);
                }
                k++;
            }

            // Фиксируем 5 строк и промежутки с минимальной дисперсией
            var fixedRowsIds = rowsMinDispersions.OrderBy(pair => pair.Value).Take(5).Select(pair => pair.Key).ToList();
            var fixedRowAndIntervals = new Dictionary<int, NoiseCalcMethodIntervalsInRowInfo>();
            foreach (var rowId in fixedRowsIds)
                fixedRowAndIntervals.Add(rowId, GetIntervalWithMinDispersion(allIntervals.Where(interval => interval.RowId == rowId).ToList()));

            int intervalLength = fixedRowAndIntervals.First().Value.IntervalEndId - fixedRowAndIntervals.First().Value.IntervalStartId + 1;

            var originalIntervals = new byte[5][];
            for (int i = 0; i < 5; i++)
            {
                var fixedInterval = fixedRowAndIntervals[fixedRowsIds[i]];
                originalIntervals[i] = new byte[intervalLength];
                for (int j = fixedInterval.IntervalStartId; j <= fixedInterval.IntervalEndId; j++)
                    originalIntervals[i][j - fixedInterval.IntervalStartId] = Params.Image.ImgArray[fixedInterval.RowId, j, (int)fixedInterval.ImgChannel];
            }

            // Сглаживаем выбранные интервалы
            var smoothedIntervals = new byte[5][];
            for (int i = 0; i < 5; i++)
            {
                var fixedInterval = fixedRowAndIntervals[fixedRowsIds[i]];
                smoothedIntervals[i] = new byte[intervalLength];

                var values = GetIntervalWithNeighbourhood(fixedInterval);
                for (int j = 0; j < intervalLength; j++)
                    smoothedIntervals[i][j] = (byte)ApplyLinearOpertorA(values[j..(j + 5)], mask5);
            }

            // Вычитаем интервалы
            var differences = new byte[5][];
            for (int i = 0; i < 5; i++)
            {
                var fixedInterval = fixedRowAndIntervals[fixedRowsIds[i]];
                var currentIntervalDiffs = new byte[intervalLength];

                for (int j = 0; j < intervalLength; j++)
                    currentIntervalDiffs[j] = (byte)(originalIntervals[i][j] - smoothedIntervals[i][j]);
                differences[i] = currentIntervalDiffs;
            }

            // Вычисляем выборочные дисперсии - п. 3
            var selectedDispersions = new double[5];
            var selectedSko = new double[5];
            for (int i = 0; i < 5; i++)
            {
                selectedDispersions[i] = MathMethods.Dispersion(differences[i]);
                selectedSko[i] = Math.Sqrt(selectedDispersions[i]);
            }

            double averageSelectedSko = MathMethods.Average(selectedSko);

            // П.5
            // СКО шума
            double noiseSko = Math.Sqrt(35 / 18) * averageSelectedSko;

            return noiseSko;
        }

        private byte[] GetIntervalWithNeighbourhood(NoiseCalcMethodIntervalsInRowInfo interval)
        {
            int intervalLength = interval.IntervalEndId - interval.IntervalStartId + 1;  // LeftLack: 2 - 0 + 1 = 3 | RightLack: 12 - 10 + 1 = 3
            var imgArray = Params.Image.ImgArray;
            var channelId = (int)interval.ImgChannel;

            var values = new byte[intervalLength + 4];  // LeftLack: [7] | RightLack: [7]
            int startInd = interval.IntervalStartId - 2;  // LeftLack: 0 - 2 = -2 | RightLack: 8
            int endInd = interval.IntervalEndId + 2;  // LeftLack: 2 + 2 = 4 | RightLack: 14

            // Если не хватает окрестности
            int lacksLeft = startInd < 0 ? Math.Abs(startInd) : 0;  // LeftLack: -2 < 0 => 2 | RightLack: 8 !< 0 => 0
            int lacksRight = endInd >= Params.Image.Width ? Math.Abs(endInd - Params.Image.Width + 1) : 0;  // LeftLack: 4 !>= 13 => 0 | RightLack: 14 >= 13 => |(14 - 13 + 1)| = 2

            for (int j = 0; j < lacksLeft; j++)
                values[j] = imgArray[interval.RowId, lacksLeft - j, channelId];
            for (int j = 0; j < lacksRight; j++)
                values[2 + intervalLength + (2 - lacksRight) + j] = imgArray[interval.RowId, Params.Image.Width - (2 - lacksRight) - lacksRight - j, channelId];

            // Заполняем основные
            for (int j = startInd + lacksLeft; j <= endInd - lacksRight; j++)
                values[j - startInd] = imgArray[interval.RowId, j, channelId];

            return values;
        }

        private List<NoiseCalcMethodIntervalsInRowInfo> GetIntervalsInRow(byte[,,] block)
        {
            if (block.GetLength(0) > 1)
                throw new Exception("Оценка шума прозводится только по строкам");

            var analysingChannels = new CommonLib.ScTypes.UniqueList<ImgChannel>() { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };
            var intervalsInfo = new List<NoiseCalcMethodIntervalsInRowInfo>();
            int width = block.GetLength(1);
            int intervalLength = width / Params.NoiseCalcMethodIntervalNumber;  // L

            foreach (var channel in analysingChannels)
            {
                for (int i = 0; i < Params.NoiseCalcMethodIntervalNumber; i++)
                {
                    int intervalStart = i * intervalLength;
                    int intervalEnd = intervalStart + intervalLength - 1;  // Т.к. нужен последний индекс интервала
                    var interval = GetRowIntervalFromBlock(block, intervalStart, intervalEnd, (int)channel);
                    double dispersion = MathMethods.Dispersion(interval);
                    intervalsInfo.Add(new NoiseCalcMethodIntervalsInRowInfo()
                    {
                        IntervalStartId = intervalStart,
                        IntervalEndId = intervalEnd,
                        Dispersion = dispersion,
                        ImgChannel = channel
                    });
                }
            }

            return intervalsInfo;
        }

        private NoiseCalcMethodIntervalsInRowInfo GetIntervalWithMinDispersion(List<NoiseCalcMethodIntervalsInRowInfo> intervals)
        {
            if (intervals.Count == 0)
                throw new Exception("Intervals for detecting interval with minimum dispersion is empty array");

            int minIntervalId = 0;
            double minDispersion = intervals[minIntervalId].Dispersion;

            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].Dispersion < minDispersion)
                {
                    minDispersion = intervals[i].Dispersion;
                    minIntervalId = i;
                }
            }

            return intervals[minIntervalId];
        }

        private byte[] GetRowIntervalFromBlock(byte[,,] block, int intervalStart, int intervalEnd, int channelId)
        {
            int intervalLength = intervalEnd - intervalStart + 1;
            byte[] result = new byte[intervalLength];

            for (int i = intervalStart; i <= intervalEnd; i++)
                result[i - intervalStart] = block[0, i, channelId];

            return result;
        }

        private double ApplyLinearOpertorA(byte[] values, double[] mask)
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
