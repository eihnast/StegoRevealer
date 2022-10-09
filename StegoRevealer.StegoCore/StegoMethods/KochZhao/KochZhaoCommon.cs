using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    /*

     */

    public static class KochZhaoCommon
    {
        // Общие методы

        // Возвращает индексы блока из сетки блоков по его линейному индексу
        public static (int, int) GetBlockByLinearIndex(int linearIndex, KochZhaoParameters parameters)
        {
            ScImageBlocks blocks = parameters.GetImgBlocksGrid();

            if (!parameters.VerticalHiding)
            {
                int line = linearIndex / blocks.BlocksInRow;
                int column = linearIndex % blocks.BlocksInRow;
                return (line, column);
            }
            else
            {
                int line = linearIndex / blocks.BlocksInColumn;
                int column = linearIndex % blocks.BlocksInColumn;
                return (line, column);
            }
        }

        // Возвращает блок по его координатам
        private static IEnumerable<byte[,]> GetBlocksIterator(
            Func<KochZhaoParameters, int?, IEnumerable<(int, int, int)>> iterator,
            KochZhaoParameters parameters, int? blocksNum = null)
        {
            int blockSize = parameters.GetBlockSize();
            foreach (var (line, column, channel) in iterator(parameters, blocksNum))
                yield return GetBlockByIndex(line, column, channel, parameters, blockSize);

            yield break;
        }

        // Изменяет заданное для скрытия/извлечения количество блоков до корректного
        private static void CorrectBlocksNum(ref int? blocksNum, KochZhaoParameters parameters)
        {
            if (!blocksNum.HasValue || blocksNum < 0 || blocksNum > parameters.GetAllBlocksNum())
                blocksNum = parameters.GetAllBlocksNum();
        }

        // Возвращает блок по его индексам
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

        // Возвращает координаты левого верхнего угла блока (координаты блока в массиве пикселей)
        public static (int y, int x) GetBlockCoords((int y, int x) gridCoords, KochZhaoParameters parameters)
        {
            return parameters.ImgBlocksGrid[gridCoords.y, gridCoords.x];
        }


        // Последовательная итерация

        // Возвращает следующий набор индексов блока определённого канала при последовательном доступе
        public static IEnumerable<(int, int, int)> GetForLinearAccessIndex(
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
                for (int k = 0; k < parameters.Channels.Count && overallCount <= blocksNum; k++)
                {
                    var (line, column) = GetBlockByLinearIndex(indexes[k], parameters);  // Индекс блока в сетке
                    var blockIndex = GetBlockCoords((line, column), parameters);
                    yield return (blockIndex.y, blockIndex.x, (int)parameters.Channels[k]);
                    overallCount++;
                    indexes[k]++;
                }

                yield break;
            }
            else  // Чересканально
            {
                while (overallCount <= blocksNum)
                {
                    for (int k = 0; k < parameters.Channels.Count && overallCount <= blocksNum; k++)
                    {
                        var (line, column) = GetBlockByLinearIndex(indexes[k], parameters);
                        var blockIndex = GetBlockCoords((line, column), parameters);
                        yield return (blockIndex.y, blockIndex.x, (int)parameters.Channels[k]);
                        overallCount++;
                        indexes[k]++;
                    }
                }

                yield break;
            }
        }

        // Возвращает следующий блок при последовательном доступе
        public static IEnumerable<byte[,]> GetForLinearAccessBlock(KochZhaoParameters parameters, int? blocksNum = null)
        {
            return GetBlocksIterator(GetForLinearAccessIndex, parameters, blocksNum);
        }


        // Псевдослучайная итерация

        // Возвращает индексы блока (левого верхнего угла) по общему линейному индексу блока
        public static (int, int, int) GetBlockIndexesFromLinearIndex(int linearIndex, KochZhaoParameters parameters)
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

            var (line, column) = GetBlockByLinearIndex(blockLinearIndex, parameters);
            return (line, column, channel);
        }

        // Возвращает индексы следующего блока при псевдослучайном доступе
        public static IEnumerable<(int, int, int)> GetForRandomAccessIndex(
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
                var (y, x, channel) = GetBlockIndexesFromLinearIndex(allLinearIndexes[i], parameters);
                var blockIndex = GetBlockCoords((y, x), parameters);
                yield return (blockIndex.y, blockIndex.x, channel);
            }

            yield break;
        }

        // Возвращает следующий блок при псевдослучайном доступе
        public static IEnumerable<byte[,]> GetForRandomAccessBlock(KochZhaoParameters parameters, int? blocksNum = null)
        {
            return GetBlocksIterator(GetForRandomAccessIndex, parameters, blocksNum);
        }


        // Преобразования, связанные с частотным представлением

        // Получение ДКП-блока
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

        // Получение ОДКП-блока
        public static double[,] IDctBlock(double[,] block, int? blockSize = null)
        {
            return MathMethods.Idct(block);
        }

        // Возвращает нормализованное значение в диапазоне [0, 255]
        public static byte NormalizeValue(double value)
        {
            if (value >= 255.0)
                return 255;
            if (value <= 0.0)
                return 0;

            return Convert.ToByte(Math.Round(value));
        }

        // Приведение ОДКП блока к массиву дискретных значений [0, 255]
        public static byte[,] NormalizeBlock(double[,] block)
        {
            var (height, width) = (block.GetLength(0), block.GetLength(1));
            var normalizedBlock = new byte[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    normalizedBlock[i, j] = NormalizeValue(block[i, j]);

            return normalizedBlock;
        }

        // Получение нормализованного ОДКП-блока
        public static byte[,] IDctBlockAndNormalize(double[,] block, int? blockSize = null)
        {
            var idctBlock = IDctBlock(block);
            return NormalizeBlock(idctBlock);
        }


        // Работа с блоком

        // Возвращает значения коэффициентов блока из переданного блока
        public static (int val1, int val2) GetBlockCoeffs(int[,] block, (int coef1, int coef2) coeffs)
        {
            return (block[coeffs.coef1, coeffs.coef2], block[coeffs.coef2, coeffs.coef1]);
        }

        public static (double val1, double val2) GetBlockCoeffs(double[,] block, (int coef1, int coef2) coeffs)
        {
            return (block[coeffs.coef1, coeffs.coef2], block[coeffs.coef2, coeffs.coef1]);
        }

        public static (int coef1, int coef2) GetCoefIndexesInImgArray(
            (int y, int x, int ch) coords, (int coef1, int coef2) coeffs)
        {
            return (coords.y + coeffs.coef1, coords.x + coeffs.coef2);
        }

        // Возвращает значения коэффициентов блока по переданным координатам и массиву пикселей
        public static (int val1, int val2) GetBlockCoeffs((int y, int x, int ch) coords, (int coef1, int coef2) coeffs,
            ImageArray imar)
        {
            (int coef1, int coef2) realCoeffs = GetCoefIndexesInImgArray(coords, coeffs);
            return (imar[realCoeffs.coef1, realCoeffs.coef2, coords.ch], 
                imar[realCoeffs.coef2, realCoeffs.coef1, coords.ch]);
        }

        // Возвращает модифицированные коэффициенты, скрывая в них бит (согласно порогу)
        // Метод перенесён из StegoAnalyzer Core (kz_common.py --> get_modified_coeffs)
        public static (double val1, double val2) GetModifiedCoeffs(
            (double val1, double val2) coeffs, int threshold, bool incrementFirst)
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
