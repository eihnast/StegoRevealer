using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ScMath;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    // TODO: описать общие принципы работы итерации и формаирования частотного представления изображения
    /*

     */

    /// <summary>
    /// Общие инструменты формирования и обхода блоков частотного представления изображения
    /// </summary>
    public static class KochZhaoCommon
    {
        // Общие методы

        /// <summary>
        /// Возвращает индексы блока из сетки блоков по его линейному индексу
        /// </summary>
        public static Sc2DPoint GetBlockByLinearIndex(int linearIndex, KochZhaoParameters parameters)
        {
            ScImageBlocks blocks = parameters.GetImgBlocksGrid();

            if (!parameters.VerticalHiding)
            {
                int line = linearIndex / blocks.BlocksInRow;
                int column = linearIndex % blocks.BlocksInRow;
                return new Sc2DPoint(line, column);
            }
            else
            {
                int column = linearIndex / blocks.BlocksInColumn;
                int line = linearIndex % blocks.BlocksInColumn;
                return new Sc2DPoint(line, column);
            }
        }

        /// <summary>
        /// Возвращает блок по его координатам
        /// </summary>
        private static IEnumerable<byte[,]> GetBlocksIterator(
            Func<KochZhaoParameters, int?, IEnumerable<ScPointCoords>> iterator,
            KochZhaoParameters parameters, int? blocksNum = null)
        {
            int blockSize = parameters.GetBlockSize();
            foreach (var blockCoords in iterator(parameters, blocksNum))
            {
                (int line, int column, int channel) = blockCoords.AsTuple();
                yield return GetBlockByIndex(line, column, channel, parameters, blockSize);
            }

            yield break;
        }

        /// <summary>
        /// Изменяет заданное для скрытия/извлечения количество блоков до корректного
        /// </summary>
        private static void CorrectBlocksNum(ref int? blocksNum, KochZhaoParameters parameters)
        {
            if (!blocksNum.HasValue || blocksNum < 0 || blocksNum > parameters.GetAllBlocksNum())
                blocksNum = parameters.GetAllBlocksNum();
        }

        /// <summary>
        /// Возвращает блок по его индексам
        /// </summary>
        public static byte[,] GetBlockByIndex(int line, int column, int channel,
            KochZhaoParameters parameters, int? blockSize = null)
        {
            if (blockSize is null)
                blockSize = parameters.GetBlockSize();

            var block = new byte[blockSize.Value, blockSize.Value];
            for (int i = line; i < line + blockSize; i++)
                for (int j = column; j < column + blockSize; j++)
                    block[i - line, j - column] = parameters.Image.ImgArray[i, j, channel];
            return block;
        }

        /// <summary>
        /// Возвращает координаты левого верхнего угла блока (координаты блока в массиве пикселей)
        /// </summary>
        public static Sc2DPoint GetBlockCoords(Sc2DPoint gridCoords, KochZhaoParameters parameters)
        {
            return parameters.ImgBlocksGrid[gridCoords.Y, gridCoords.X];
        }


        // Последовательная итерация

        /// <summary>
        /// Возвращает следующий набор индексов блока определённого канала при последовательном доступе
        /// </summary>
        public static IEnumerable<ScPointCoords> GetForLinearAccessIndex(
            KochZhaoParameters parameters, int? blocksNum = null)
        {
            int overallCount = 0;
            CorrectBlocksNum(ref blocksNum, parameters);

            // Стартовые линейные индексы блоков
            int[] indexes = new int[parameters.Channels.Count];
            for (int i = 0; i < parameters.Channels.Count; i++)
                indexes[i] = parameters.StartBlocks[i];

            if (!parameters.InterlaceChannels)  // Поканально
            {
                int channelIndex = 0;
                while (channelIndex < parameters.Channels.Count && overallCount <= blocksNum)
                {
                    var (line, column) = GetBlockByLinearIndex(indexes[channelIndex], parameters).AsTuple();  // Индекс блока в сетке
                    var blockIndex = GetBlockCoords(new Sc2DPoint(line, column), parameters);
                    yield return new ScPointCoords(blockIndex.Y, blockIndex.X, (int)parameters.Channels[channelIndex]);
                    overallCount++;
                    indexes[channelIndex]++;

                    if (line == parameters.ImgBlocksGrid.BlocksInColumn - 1 && column == parameters.ImgBlocksGrid.BlocksInRow - 1)
                        channelIndex++;
                }

                yield break;
            }
            else  // Чересканально
            {
                while (overallCount <= blocksNum)
                {
                    for (int k = 0; k < parameters.Channels.Count && overallCount <= blocksNum; k++)
                    {
                        var (line, column) = GetBlockByLinearIndex(indexes[k], parameters).AsTuple();
                        var blockIndex = GetBlockCoords(new Sc2DPoint(line, column), parameters);
                        yield return new ScPointCoords(blockIndex.Y, blockIndex.X, (int)parameters.Channels[k]);
                        overallCount++;
                        indexes[k]++;
                    }
                }

                yield break;
            }
        }

        /// <summary>
        /// Возвращает следующий блок при последовательном доступе
        /// </summary>
        public static IEnumerable<byte[,]> GetForLinearAccessBlock(KochZhaoParameters parameters, int? blocksNum = null)
        {
            return GetBlocksIterator(GetForLinearAccessIndex, parameters, blocksNum);
        }


        // Псевдослучайная итерация

        /// <summary>
        /// Возвращает индексы блока (левого верхнего угла) по общему линейному индексу блока
        /// </summary>
        public static ScPointCoords GetBlockIndexesFromLinearIndex(int linearIndex, KochZhaoParameters parameters)
        {
            // Вычисление происходит в зависимости от: обхода по матрице, чересканальности

            // Определение индекса канала и линейного индекса блока
            int channel;
            int blockLinearIndex;
            if (parameters.InterlaceChannels)  // Чередование каналов (Block1 {R;G;B} --> 0,1,2)
            {
                channel = (int)parameters.Channels[linearIndex % parameters.Channels.Count];
                var rawIndexValue = (decimal)linearIndex / parameters.Channels.Count;
                blockLinearIndex = Convert.ToInt32(Math.Round(rawIndexValue, MidpointRounding.ToPositiveInfinity));
            }
            else  // Поканально (R: Block1, Block2, Block3 --> 0,1,2)
            {
                var blocksGrid = parameters.GetImgBlocksGrid();
                var (w, h) = (blocksGrid.BlocksInRow, blocksGrid.BlocksInColumn);
                int channelInnerIndex = linearIndex / (w * h);
                channel = (int)parameters.Channels[channelInnerIndex];
                blockLinearIndex = linearIndex - channelInnerIndex * (w * h);
            }

            var (line, column) = GetBlockByLinearIndex(blockLinearIndex, parameters).AsTuple();
            return new ScPointCoords(line, column, channel);
        }

        /// <summary>
        /// Возвращает индексы следующего блока при псевдослучайном доступе
        /// </summary>
        public static IEnumerable<ScPointCoords> GetForRandomAccessIndex(
            KochZhaoParameters parameters, int? blocksNum = null)
        {
            ScImageBlocks blocksGrid = parameters.GetImgBlocksGrid();
            var rnd = parameters.Seed.HasValue ? new Random(parameters.Seed.Value) : new Random();

            int blocksLinearLength = parameters.GetAllBlocksNum();
            CorrectBlocksNum(ref blocksNum, parameters);

            // Массив общих линейных индексов блоков
            var allLinearIndexes = Enumerable.Range(0, blocksLinearLength).ToArray();  // Формирование
            allLinearIndexes = allLinearIndexes.OrderBy(e => rnd.Next()).ToArray();  // Перемешивание

            for (int i = 0; i < blocksNum; i++)
            {
                var (y, x, channel) = GetBlockIndexesFromLinearIndex(allLinearIndexes[i], parameters).AsTuple();
                var blockIndex = GetBlockCoords(new Sc2DPoint(y, x), parameters);
                yield return new ScPointCoords(blockIndex.Y, blockIndex.X, channel);
            }

            yield break;
        }

        /// <summary>
        /// Возвращает следующий блок при псевдослучайном доступе
        /// </summary>
        public static IEnumerable<byte[,]> GetForRandomAccessBlock(KochZhaoParameters parameters, int? blocksNum = null)
        {
            return GetBlocksIterator(GetForRandomAccessIndex, parameters, blocksNum);
        }


        // Преобразования, связанные с частотным представлением

        /// <summary>
        /// Получение ДКП-блока
        /// </summary>
        public static double[,] DctBlock(byte[,] block, int? blockSize = null)
        {
            if (blockSize is null)
                blockSize = block.GetLength(0);

            double[,] doubleBlock = new double[blockSize.Value, blockSize.Value];
            for (int i = 0; i < blockSize.Value; i++)
                for (int j = 0; j < blockSize.Value; j++)
                    doubleBlock[i, j] = Convert.ToDouble(block[i, j]);

            return MathMethods.Dct(doubleBlock);
        }

        /// <summary>
        /// Получение ОДКП-блока
        /// </summary>
        public static double[,] IDctBlock(double[,] block, int? blockSize = null)
        {
            return MathMethods.Idct(block);
        }

        /// <summary>
        /// Возвращает нормализованное значение в диапазоне [0, 255]
        public static byte NormalizeValue(double value)
        {
            if (value >= 255.0)
                return 255;
            if (value <= 0.0)
                return 0;

            return Convert.ToByte(Math.Round(value));
        }

        /// <summary>
        /// Приведение ОДКП блока к массиву дискретных значений [0, 255]
        /// </summary>
        public static byte[,] NormalizeBlock(double[,] block)
        {
            var (height, width) = (block.GetLength(0), block.GetLength(1));
            var normalizedBlock = new byte[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    normalizedBlock[i, j] = NormalizeValue(block[i, j]);

            return normalizedBlock;
        }

        /// <summary>
        /// Получение нормализованного ОДКП-блока
        /// </summary>
        public static byte[,] IDctBlockAndNormalize(double[,] block, int? blockSize = null)
        {
            var idctBlock = IDctBlock(block);
            return NormalizeBlock(idctBlock);
        }


        // Работа с блоком

        /// <summary>
        /// Возвращает значения коэффициентов блока из переданного блока
        /// </summary>
        public static (int val1, int val2) GetBlockCoeffs(int[,] block, ScIndexPair coeffs)
        {
            return (block[coeffs.FirstIndex, coeffs.SecondIndex], block[coeffs.SecondIndex, coeffs.FirstIndex]);
        }

        /// <summary>
        /// Возвращает значения коэффициентов блока из переданного блока
        /// </summary>
        public static (double val1, double val2) GetBlockCoeffs(double[,] block, ScIndexPair coeffs)
        {
            return (block[coeffs.FirstIndex, coeffs.SecondIndex], block[coeffs.SecondIndex, coeffs.FirstIndex]);
        }

        public static ScIndexPair GetCoefIndexesInImgArray(ScPointCoords coords, ScIndexPair coeffs) =>
            new ScIndexPair(coords.Y + coeffs.FirstIndex, coords.X + coeffs.SecondIndex);

        /// <summary>
        /// Возвращает значения коэффициентов блока по переданным координатам и массиву пикселей
        /// </summary>
        public static (int val1, int val2) GetBlockCoeffs(ScPointCoords coords, ScIndexPair coeffs,
            ImageArray imar)
        {
            ScIndexPair realCoeffs = GetCoefIndexesInImgArray(coords, coeffs);
            return (imar[realCoeffs.FirstIndex, realCoeffs.SecondIndex, coords.ChannelId], 
                imar[realCoeffs.SecondIndex, realCoeffs.FirstIndex, coords.ChannelId]);
        }

        /// <summary>
        /// Возвращает модифицированные коэффициенты, скрывая в них бит (согласно порогу)<br/>
        /// Метод перенесён из StegoAnalyzer Core (kz_common.py --> get_modified_coeffs)
        /// </summary>
        public static (double val1, double val2) GetModifiedCoeffs(
            (double val1, double val2) coeffs, double threshold, bool incrementFirst)
        {
            var (coefVal1, coefVal2) = coeffs;  // Модифицируемые значения коэффициентов
            var difference = MathMethods.GetModulesDiff(coefVal1, coefVal2);  // Разница значений коэффициентов

            if (incrementFirst)
            {
                while (difference <= threshold)
                {
                    coefVal1++;
                    if (coefVal2 > 0)
                        coefVal2--;
                    difference = MathMethods.GetModulesDiff(coefVal1, coefVal2);
                }
            }
            else
            {
                while (difference >= threshold)
                {
                    coefVal2++;
                    if (coefVal1 > 0)
                        coefVal1--;
                    difference = MathMethods.GetModulesDiff(coefVal1, coefVal2);
                }
            }

            if (coeffs.val1 < 0)
                coefVal1 = -coefVal1;
            if (coeffs.val2 < 0)
                coefVal2 = -coefVal2;

            return (coefVal1, coefVal2);
        }
    }
}
