using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.ImageHandlerLib.Blocks
{
    /*
     * Предлагаются два вида итераторов для последовательного и псевдослучайного доступа:
     * - первый возвращает координаты блока в матрице блоков (и индекс цветового канала) - структуру ScPointCoords;
     * - второй возвращает непосредственно байтовые значения одноканального блока в нужном канале - массив byte[,].
     * Для получения координат пикселей блока (левого верхнего и правого нижнего) достаточно использовать первый из описанных итераторов
     *   для получения индексов ScPointCoords indexes, после чего получить координаты пикселей как blocks[indexes.Y, indexes.X]
     * 
     * Реализованы два метода получения индексов блока по линейному индексу:
     * - первый учитывает только способ обхода по 2D-матрице, т.е. предполагает, что линейный индекс существует
     *   в массиве типа "строка за строкой" или "столбец за столбцом" (строки и столбцы матрицы блоков)
     * - второй учитывает также и чересканальность, т.е. предполагает, что общий линейный индекс существует
     *   в массиве длиной ВЫСОТА_МАТРИЦЫ * ШИРИНА_МАТРИЦЫ * КОЛИЧЕСТВО_КАНАЛОВ, где собраны все возможные одноканальные блоки
     * С первым вариантом проще работать при последовательном скрытии: индексы блоков можно точно рассчитать в зависимости от
     *   канала, со вторым - при псевдослучайном, где скрытие производится в случайный одноканальный блок, и необходима для
     *   воспроизведения последовательности по ключу ГПСЧ вся их последовательность.
     */

    /// <summary>
    /// Методы обхода матрицы блоков
    /// </summary>
    public static class BlocksTraverseHelper
    {
        // Общие методы

        /// <summary>
        /// Возвращает индексы блока в матрице блоков по его линейному индексу<br/>
        /// <i>Не учитывает каналы - работает непосредственно с 2D-матрицей разбиения пикселей на блоки</i>
        /// </summary>
        public static Sc2DPoint GetBlockIndexesBy2DLinearIndex(int linearIndex, ImageBlocks blocks, TraverseType traverseType = TraverseType.Horizontal)
        {
            if (traverseType is TraverseType.Horizontal)
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
        /// Возвращает индексы блока в матрице по общему линейному индексу блока<br/>
        /// <i>Общий линейный индекс блока учитывает также цветовой канал - это индекс блока в общем списке одноцветовых блоков</i><br/>
        /// Расчёт, таким образом, зависит не только от метода обхода матрицы, но и от флага чересканального считывания
        /// </summary>
        public static ScPointCoords GetBlockIndexesByFullLinearIndex(int linearIndex, ImageBlocks blocks, BlocksTraverseOptions options)
        {
            // Вычисление происходит в зависимости от: обхода по матрице, чересканальности

            // Определение индекса канала и линейного индекса блока
            int channel;
            int blockLinearIndex;
            if (options.InterlaceChannels)  // Чередование каналов (Block1 {R;G;B} --> 0,1,2)
            {
                channel = (int)options.Channels[linearIndex % options.Channels.Count];
                var rawIndexValue = (decimal)linearIndex / options.Channels.Count;
                blockLinearIndex = Convert.ToInt32(Math.Round(rawIndexValue, MidpointRounding.ToPositiveInfinity));
            }
            else  // Поканально (R: Block1, Block2, Block3 --> 0,1,2)
            {
                var (w, h) = (blocks.BlocksInRow, blocks.BlocksInColumn);
                int channelInnerIndex = linearIndex / (w * h);
                channel = (int)options.Channels[channelInnerIndex];
                blockLinearIndex = linearIndex - channelInnerIndex * (w * h);
            }

            var (line, column) = GetBlockIndexesBy2DLinearIndex(blockLinearIndex, blocks).AsTuple();
            return new ScPointCoords(line, column, channel);
        }

        /// <summary>
        /// Возвращает итератор блоков (байтовых значений блоков) по их индексам в матрице блоков и индексу канала
        /// </summary>
        private static IEnumerable<byte[,]> GetBlocksIterator(
            Func<ImageBlocks, BlocksTraverseOptions, int?, IEnumerable<ScPointCoords>> iterator,
            ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            foreach (var blockCoords in iterator(blocks, options, blocksNum))
                yield return GetBlockByIndexes(blockCoords, blocks);
            yield break;
        }

        /// <summary>
        /// Изменяет некорректно заданное для скрытия/извлечения количество блоков на максимальное
        /// </summary>
        private static void CorrectBlocksNum(ref int? blocksNum, ImageBlocks blocks)
        {
            if (!blocksNum.HasValue || blocksNum < 0 || blocksNum > blocks.BlocksNum)
                blocksNum = blocks.BlocksNum;
        }

        /// <summary>
        /// Возвращает блок по его индексам в матрице блоков и индексу канала
        /// </summary>
        public static byte[,] GetBlockByIndexes(ScPointCoords blockIndexes, ImageBlocks blocks)
        {
            (int line, int column, int channelId) = blockIndexes.AsTuple();

            var blockPixelsIndexes = blocks[line, column];
            var block = new byte[blocks.BlockHeight, blocks.BlockWidth];

            for (int i = blockPixelsIndexes.Lt.Y; i < blockPixelsIndexes.Rd.Y; i++)
                for (int j = blockPixelsIndexes.Lt.X; j < blockPixelsIndexes.Rd.X; j++)
                    block[i - line, j - column] = blocks.Image.ImgArray[i, j, channelId];

            return block;
        }


        // Последовательная итерация

        /// <summary>
        /// Возвращает следующий набор индексов блока в матрице блоков при последовательном доступе
        /// </summary>
        public static IEnumerable<ScPointCoords> GetForLinearAccessIndexes(
            ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            int overallCount = 0;
            CorrectBlocksNum(ref blocksNum, blocks);

            // Стартовые линейные индексы блоков
            int[] indexes = new int[options.Channels.Count];  // Хранит линейный индекс текущего рабочего блока для каждого канала
            for (int i = 0; i < options.Channels.Count; i++)
                indexes[i] = options.StartBlocks[i];

            if (!options.InterlaceChannels)  // Поканально
            {
                int channelIndex = 0;
                while (channelIndex < options.Channels.Count && overallCount <= blocksNum)
                {
                    var (line, column) = GetBlockIndexesBy2DLinearIndex(indexes[channelIndex], blocks).AsTuple();  // Индексы блока в матрице
                    yield return new ScPointCoords(line, column, (int)options.Channels[channelIndex]);
                    overallCount++;
                    indexes[channelIndex]++;

                    if (line == blocks.BlocksInColumn - 1 && column == blocks.BlocksInRow - 1)
                        channelIndex++;  // Придостижении последнего блока в 2D матрице переходим к следующему каналу
                }

                yield break;
            }
            else  // Чересканально
            {
                while (overallCount <= blocksNum)
                {
                    for (int k = 0; k < options.Channels.Count && overallCount <= blocksNum; k++)
                    {
                        var (line, column) = GetBlockIndexesBy2DLinearIndex(indexes[k], blocks).AsTuple();  // Индексы блока в матрице
                        yield return new ScPointCoords(line, column, (int)options.Channels[k]);
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
        public static IEnumerable<byte[,]> GetForLinearAccessBlock(ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            return GetBlocksIterator(GetForLinearAccessIndexes, blocks, options, blocksNum);
        }


        // Псевдослучайная итерация

        /// <summary>
        /// Возвращает следующий набор индексов блока в матрице блоков при псевдослучайном доступе
        /// </summary>
        public static IEnumerable<ScPointCoords> GetForRandomAccessIndexes(
            ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            var rnd = options.Seed.HasValue ? new Random(options.Seed.Value) : new Random();

            int blocksLinearLength = blocks.BlocksNum;
            CorrectBlocksNum(ref blocksNum, blocks);

            // Массив общих линейных индексов блоков
            var allLinearIndexes = Enumerable.Range(0, blocksLinearLength).ToArray();  // Формирование
            allLinearIndexes = allLinearIndexes.OrderBy(e => rnd.Next()).ToArray();  // Перемешивание

            for (int i = 0; i < blocksNum; i++)
            {
                var (y, x, channelId) = GetBlockIndexesByFullLinearIndex(allLinearIndexes[i], blocks, options).AsTuple();
                yield return new ScPointCoords(y, x, channelId);
            }

            yield break;
        }

        /// <summary>
        /// Возвращает следующий блок при псевдослучайном доступе
        /// </summary>
        public static IEnumerable<byte[,]> GetForRandomAccessBlock(ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            return GetBlocksIterator(GetForRandomAccessIndexes, blocks, options, blocksNum);
        }
    }
}
