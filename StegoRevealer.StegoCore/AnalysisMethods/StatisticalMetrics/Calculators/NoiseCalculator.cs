using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ScMath;
using StegoRevealer.StegoCore.StegoMethods;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using System.Threading.Channels;
using StegoRevealer.StegoCore.CommonLib.Entities;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class NoiseCalculator
{
    private StatmParameters _params;

    public NoiseCalculator(StatmParameters parameters)
    {
        _params = parameters;
    }


    // https://cyberleninka.ru/article/n/metod-otsenki-urovnya-shuma-tsifrovogo-izobrazheniya/viewer

    private double[] mask5 = new double[5] { -3 / 35, 12 / 35, 17 / 35, 12 / 35, -3 / 35 };
    private double[] mask7 = new double[7] { -2 / 21, 3 / 21, 6 / 21, 7 / 21, 6 / 21, 3 / 21, -2 / 21 };


    public enum NoiseCalculationMethod
    {
        Method1,
        Method2
    }


    /// <summary>
    /// Оценка шума
    /// </summary>
    public double CalcNoiseLevel(NoiseCalculationMethod method)
    {
        switch (method)
        {
            case NoiseCalculationMethod.Method1:
                return CalcNoiseLevelMethod1();
            case NoiseCalculationMethod.Method2:
                return CalcNoiseLevelMethod2();
            default:
                return CalcNoiseLevelMethod2();
        }
    }


    // Оценка шума. Метод 2.
    private double CalcNoiseLevelMethod2()
    {
        var blocks = new ImageBlocks(new ImageBlocksParameters(_params.Image, _params.Image.Width, 1));

        // Если недостаточно высоты изображения для выбора строк с заданным шагом (по умолчанию - 50)
        if (blocks.BlocksInColumn < _params.NoiseCalcSteps * _params.NoiseCalcStepsDivider)
            _params.NoiseCalcSteps = blocks.BlocksInColumn / _params.NoiseCalcStepsDivider;

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
            if (k % _params.NoiseCalcSteps == 0)
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
                originalIntervals[i][j - fixedInterval.IntervalStartId] = _params.Image.ImgArray[fixedInterval.RowId, j, (int)fixedInterval.ImgChannel];
        }

        // Сглаживаем выбранные интервалы
        var smoothedIntervals = new byte[5][];
        for (int i = 0; i < 5; i++)
        {
            var fixedInterval = fixedRowAndIntervals[fixedRowsIds[i]];
            smoothedIntervals[i] = new byte[intervalLength];

            var values = PixelsTools.GetIntervalWithNeighbourhood(_params.Image, fixedInterval, 2);
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


    // Оценка шума. Метод 1.
    private double CalcNoiseLevelMethod1()
    {
        var allChannelsList = new UniqueList<ImgChannel>() { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };


        // П.1. Разбиваем изображение на блоки
        int minSize = Math.Min(_params.Image.Width, _params.Image.Height);
        int blockSize = Math.Max(_params.Image.Width, _params.Image.Height) / _params.NoiseCalcBlocksNumber;
        if (blockSize > minSize)
            blockSize = minSize;

        var blocks = new ImageBlocks(new ImageBlocksParameters(_params.Image, blockSize));

        // П.1. Вычисление дисперсии в каждом блоке
        var dispersions = new Dictionary<Sc2DPoint, double>();
        for (int i = 0; i < blocks.BlocksInColumn; i++)
            for (int j = 0; j < blocks.BlocksInRow; j++)
            {
                double redDispersion = MathMethods.Dispersion(ImageBlocks.GetOneChannelBlockByBlockIndexes(blocks, (i, j), ImgChannel.Red));
                double greenDispersion = MathMethods.Dispersion(ImageBlocks.GetOneChannelBlockByBlockIndexes(blocks, (i, j), ImgChannel.Green));
                double blueDispersion = MathMethods.Dispersion(ImageBlocks.GetOneChannelBlockByBlockIndexes(blocks, (i, j), ImgChannel.Blue));
                dispersions.Add(new Sc2DPoint(i, j), MathMethods.Average(new double[] { redDispersion, greenDispersion, blueDispersion }));
            }

        // П.1. Фиксация 5 блоков с минимальными дисперсиями: в данном случае если их меньше 5 - работаем с имеющимся количеством (исходя из описания метода, можно и с 1)
        var minDispersionsBlocks = dispersions.OrderBy(val => val.Value).Take(Math.Min(5, dispersions.Count())).ToList();
        var fixedBlocks = minDispersionsBlocks.Select(val => val.Key).ToList();


        // П.2. Фиксируем в каждом блоке строки
        // Фиксируем по каждому выбранному индексу строки фактически 3 строки - для каждого канала
        var fixedRows = new Dictionary<Sc2DPoint, List<ImageHorizontalIntervalInfo>>();
        foreach (var fixedBlock in fixedBlocks)
        {
            var block = ImageBlocks.GetBlockByBlockIndexes(blocks, fixedBlock, allChannelsList);
            int rowsNum = block.GetLength(0);
            int rowsInBlock = Math.Min(_params.NoiseCalcRowsInBlock, rowsNum);  // Если в блоке вообще нет такого числа строк

            int rowsStep = rowsNum / rowsInBlock;
            var intervals = new List<ImageHorizontalIntervalInfo>();

            var blockCoords = blocks[fixedBlock.Y, fixedBlock.X];
            int blockStartRow = blockCoords.Lt.Y;

            for (int i = 0; i < rowsInBlock; i++)
            {
                int rowIndex = i * rowsStep;
                for (int channelId = 0; channelId < allChannelsList.Count; channelId++)
                {
                    intervals.Add(new ImageHorizontalIntervalInfo()
                    {
                        RowId = blockStartRow + rowIndex,
                        ImgChannel = allChannelsList[channelId],
                        IntervalStartId = blockCoords.Lt.X,
                        IntervalEndId = blockCoords.Rd.X
                    });
                }
            }

            fixedRows.Add(fixedBlock, intervals);
        }


        // П.2. Сглаживаем зафиксированные строки в выбранных блоках разностным оператором (B = A1-5 - A1-7)
        var smoothedImage = _params.Image.Clone();
        foreach (var rowInfo in fixedRows)
        {
            foreach (var rowInBlock in rowInfo.Value)
            {
                int intervalLength = rowInBlock.IntervalEndId - rowInBlock.IntervalStartId + 1;
                var smoothedRowA15 = new byte[intervalLength];
                var smoothedRowA17 = new byte[intervalLength];

                var valuesA15 = PixelsTools.GetIntervalWithNeighbourhood(_params.Image, rowInBlock, 2);
                var valuesA17 = PixelsTools.GetIntervalWithNeighbourhood(_params.Image, rowInBlock, 3);
                for (int j = 0; j < intervalLength; j++)
                    smoothedRowA15[j] = (byte)ApplyLinearOpertorA(valuesA15[j..(j + 5)], mask5);
                for (int j = 0; j < intervalLength; j++)
                    smoothedRowA17[j] = (byte)ApplyLinearOpertorA(valuesA15[j..(j + 7)], mask7);

                var smoothedRow = new byte[intervalLength];
                for (int j = 0; j < intervalLength; j++)
                    smoothedRow[j] = (byte)Math.Max(0, smoothedRowA15[j] - smoothedRowA17[j]);

                // Вписываем сглаженные значения в изображение
                for (int j = 0; j < intervalLength; j++)
                    smoothedImage.ImgArray[rowInBlock.RowId, rowInBlock.IntervalStartId + j, (int)rowInBlock.ImgChannel] = smoothedRow[j];
            }
        }


        // П.3. В зафкисированных блоках вычисляем выборочные дисперсии и СКО
        var selectedDispersions = new List<double>();
        var selectedSko = new List<double>();
        var smoothedImageBlocks = new ImageBlocks(new ImageBlocksParameters(smoothedImage, blockSize));
        foreach (var fixedBlock in fixedBlocks)
        {
            double redDispersion = MathMethods.Dispersion(ImageBlocks.GetOneChannelBlockByBlockIndexes(smoothedImageBlocks, fixedBlock, ImgChannel.Red));
            double greenDispersion = MathMethods.Dispersion(ImageBlocks.GetOneChannelBlockByBlockIndexes(smoothedImageBlocks, fixedBlock, ImgChannel.Green));
            double blueDispersion = MathMethods.Dispersion(ImageBlocks.GetOneChannelBlockByBlockIndexes(smoothedImageBlocks, fixedBlock, ImgChannel.Blue));

            double dispersion = MathMethods.Average(new double[] { redDispersion, greenDispersion, blueDispersion });
            selectedDispersions.Add(dispersion);
            selectedSko.Add(Math.Sqrt(dispersion));
        }

        double averageSelectedSko = MathMethods.Average(selectedSko.ToArray());


        // П.4. Вычисление оценки шума
        double noiseSko = (Math.Sqrt(105) / 4) * averageSelectedSko;

        return noiseSko;
    }


    #region Helpers

    // Разбивает строку (переданную в формате блока) на набор горизонтальных интервалов
    private List<NoiseCalcMethodIntervalsInRowInfo> GetIntervalsInRow(byte[,,] block)
    {
        if (block.GetLength(0) > 1)
            throw new Exception("Оценка шума прозводится только по строкам");

        var analysingChannels = new CommonLib.ScTypes.UniqueList<ImgChannel>() { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };
        var intervalsInfo = new List<NoiseCalcMethodIntervalsInRowInfo>();
        int width = block.GetLength(1);
        int intervalLength = width / _params.NoiseCalcIntervalNumber;  // L

        foreach (var channel in analysingChannels)
        {
            for (int i = 0; i < _params.NoiseCalcIntervalNumber; i++)
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


    // Возвращает информацию об интервале с минимальной дисперсией
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

    // Возвращает значения пикселей на указанном интервале (последовательность значений)
    private byte[] GetRowIntervalFromBlock(byte[,,] block, int intervalStart, int intervalEnd, int channelId)
    {
        int intervalLength = intervalEnd - intervalStart + 1;
        byte[] result = new byte[intervalLength];

        for (int i = intervalStart; i <= intervalEnd; i++)
            result[i - intervalStart] = block[0, i, channelId];

        return result;
    }

    // Применяет линейный оператор к последовательности (интервалу)
    private double ApplyLinearOpertorA(byte[] values, double[] mask)
    {
        if (values.Length != mask.Length)
            throw new ArgumentException("Длины отрезка значений и маски должны быть равны");

        double result = 0.0;
        for (int i = 0; i < mask.Length; i++)
            result += mask[i] * values[i];

        return result;
    }

    #endregion
}
