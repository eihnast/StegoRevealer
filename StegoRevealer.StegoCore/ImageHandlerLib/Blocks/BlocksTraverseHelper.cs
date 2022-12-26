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
     *   
     * Существует две основные группы методов:
     *   первая возвращает индексы блоков/блоки в одноканальном режиме (три индекса блока вместе с индексом канала и массивы byte[,])
     *   вторая - индексы блоков/блоки в многоканальном режиме (два индекса блока и массивы byte[,,])
     * От применения этих методов зависит применение определённых параметров обхода: при обходе многоканальных блоков (просто "блоков")
     *   не применяются такие параметры, как чересканальность - блок получается и учитывается при обходе целиком. Фактически, это обход
     *   самой матрицы блоков как таковой, без специфики каналов.
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
        /// Возвращает итератор одноканальных блоков (байтовых значений блоков) по их индексам в матрице блоков и индексу канала
        /// </summary>
        private static IEnumerable<byte[,]> GetOneChannelBlocksIterator(
            Func<ImageBlocks, BlocksTraverseOptions, int?, IEnumerable<ScPointCoords>> iterator,
            ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            foreach (var blockCoords in iterator(blocks, options, blocksNum))
                yield return GetOneChannelBlockByIndexes(blockCoords, blocks);
            yield break;
        }

        /// <summary>
        /// Возвращает итератор блоков (байтовых значений блоков) по их индексам в матрице блоков и индексу канала
        /// </summary>
        private static IEnumerable<byte[,,]> GetBlocksIterator(
            Func<ImageBlocks, BlocksTraverseOptions, int?, IEnumerable<Sc2DPoint>> iterator,
            ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            foreach (var blockCoords in iterator(blocks, options, blocksNum))
                yield return GetBlockByIndexes(blockCoords, blocks, options.Channels);
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
        /// Возвращает одноканальный блок по его индексам в матрице блоков и индексу канала
        /// </summary>
        public static byte[,] GetOneChannelBlockByIndexes(ScPointCoords blockIndexes, ImageBlocks blocks)
        {
            (int line, int column, int channelId) = blockIndexes.AsTuple();

            var blockPixelsIndexes = blocks[line, column];
            var block = new byte[blocks.BlockHeight, blocks.BlockWidth];

            for (int i = blockPixelsIndexes.Lt.Y; i <= blockPixelsIndexes.Rd.Y; i++)
                for (int j = blockPixelsIndexes.Lt.X; j <= blockPixelsIndexes.Rd.X; j++)
                    block[i - blockPixelsIndexes.Lt.Y, j - blockPixelsIndexes.Lt.X] = blocks.Image.ImgArray[i, j, channelId];

            return block;
        }

        /// <summary>
        /// Возвращает полноценный блок по его индексам в матрице блоков и индексу канала
        /// </summary>
        public static byte[,,] GetBlockByIndexes(Sc2DPoint blockIndexes, ImageBlocks blocks, UniqueList<ImgChannel> channels)
        {
            (int line, int column) = blockIndexes.AsTuple();

            var blockPixelsIndexes = blocks[line, column];
            var block = new byte[blocks.BlockHeight, blocks.BlockWidth, channels.Count];

            for (int i = blockPixelsIndexes.Lt.Y; i <= blockPixelsIndexes.Rd.Y; i++)
                for (int j = blockPixelsIndexes.Lt.X; j <= blockPixelsIndexes.Rd.X; j++)
                    for (int channelId = 0; i < channels.Count; channelId++)
                        block[i - blockPixelsIndexes.Lt.Y, j - blockPixelsIndexes.Lt.X, channelId] = blocks.Image.ImgArray[i, j, channelId];

            return block;
        }


        // Последовательная итерация

        /// <summary>
        /// Возвращает следующий набор индексов одноканального блока в матрице блоков при последовательном доступе
        /// </summary>
        public static IEnumerable<ScPointCoords> GetForLinearAccessOneChannelBlocksIndexes(
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
                    var (line, column) = GetBlockIndexesBy2DLinearIndex(indexes[channelIndex], blocks, options.TraverseType).AsTuple();  // Индексы блока в матрице
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
                        var (line, column) = GetBlockIndexesBy2DLinearIndex(indexes[k], blocks, options.TraverseType).AsTuple();  // Индексы блока в матрице
                        yield return new ScPointCoords(line, column, (int)options.Channels[k]);
                        overallCount++;
                        indexes[k]++;
                    }
                }

                yield break;
            }
        }

        /// <summary>
        /// Возвращает следующий одноканальный блок при последовательном доступе
        /// </summary>
        public static IEnumerable<byte[,]> GetForLinearAccessOneChannelBlocks(ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            return GetOneChannelBlocksIterator(GetForLinearAccessOneChannelBlocksIndexes, blocks, options, blocksNum);
        }

        /// <summary>
        /// Возвращает следующий набор индексов блока в матрице блоков при последовательном доступе
        /// </summary>
        public static IEnumerable<Sc2DPoint> GetForLinearAccessBlocksIndexes(
            ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            int overallCount = 0;
            CorrectBlocksNum(ref blocksNum, blocks);

            // Опции поканальности и чересканальности не оказывают влияния (т.к. обход блоков целиком, со значениями всех требуемых каналов)
            // Опции выбора стартовых блоков не имеют влияния (т.к. могут быть заданы разными для разных каналов)
            while (overallCount <= blocksNum)
            {
                var (line, column) = GetBlockIndexesBy2DLinearIndex(overallCount, blocks, options.TraverseType).AsTuple();  // Индексы блока в матрице
                yield return new Sc2DPoint(line, column);
                overallCount++;
            }

            yield break;
        }

        /// <summary>
        /// Возвращает следующий блок при последовательном доступе
        /// </summary>
        public static IEnumerable<byte[,,]> GetForLinearAccessBlocks(ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            return GetBlocksIterator(GetForLinearAccessBlocksIndexes, blocks, options, blocksNum);
        }


        // Псевдослучайная итерация

        /// <summary>
        /// Возвращает следующий набор индексов одноканального блока в матрице блоков при псевдослучайном доступе
        /// </summary>
        public static IEnumerable<ScPointCoords> GetForRandomAccessOneChannelBlocksIndexes(
            ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            var rnd = options.Seed.HasValue ? new Random(options.Seed.Value) : new Random();

            int blocksLinearLength = blocks.BlocksNum * options.Channels.Count;
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
        /// Возвращает следующий одноканальный блок при псевдослучайном доступе
        /// </summary>
        public static IEnumerable<byte[,]> GetForRandomAccessOneChannelBlocks(ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            return GetOneChannelBlocksIterator(GetForRandomAccessOneChannelBlocksIndexes, blocks, options, blocksNum);
        }

        /// <summary>
        /// Возвращает следующий набор индексов блока в матрице блоков при псевдослучайном доступе
        /// </summary>
        public static IEnumerable<Sc2DPoint> GetForRandomAccessBlocksIndexes(
            ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            var rnd = options.Seed.HasValue ? new Random(options.Seed.Value) : new Random();

            int blocksLinearLength = blocks.BlocksNum;  // Псевдослучайный обход в данном контексте - только по матрице полноценных блоков
            CorrectBlocksNum(ref blocksNum, blocks);

            // Массив общих линейных индексов блоков
            var allLinearIndexes = Enumerable.Range(0, blocksLinearLength).ToArray();  // Формирование
            allLinearIndexes = allLinearIndexes.OrderBy(e => rnd.Next()).ToArray();  // Перемешивание

            for (int i = 0; i < blocksNum; i++)
            {
                var (y, x) = GetBlockIndexesBy2DLinearIndex(allLinearIndexes[i], blocks, options.TraverseType).AsTuple();
                yield return new Sc2DPoint(y, x);
            }

            yield break;
        }

        /// <summary>
        /// Возвращает следующий блок при псевдослучайном доступе
        /// </summary>
        public static IEnumerable<byte[,,]> GetForRandomAccessBlocks(ImageBlocks blocks, BlocksTraverseOptions options, int? blocksNum = null)
        {
            return GetBlocksIterator(GetForRandomAccessBlocksIndexes, blocks, options, blocksNum);
        }
    }
}
