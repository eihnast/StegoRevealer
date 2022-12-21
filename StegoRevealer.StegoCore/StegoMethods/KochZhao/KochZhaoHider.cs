using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    /// <summary>
    /// Скрытие информации по методу Коха-Жао
    /// </summary>
    public class KochZhaoHider : IHider
    {
        /// <summary>
        /// Параметры метода Коха-Жао
        /// </summary>
        public KochZhaoParameters Params { get; set; }


        public KochZhaoHider(ImageHandler img)
        {
            Params = new KochZhaoParameters(img);
        }


        /// <summary>
        /// Допустимый объём контейнера
        /// </summary>
        private int GetContainerVolume()
        {
            var blocksNum = Params.GetAllBlocksNum();
            return blocksNum;
        }

        /// <summary>
        /// Определение скрываемого объёма информации (учитывает допустимый объём контейнера)
        /// </summary>
        private static int GetHidingVolume(int ContainerVolume, int DataVolume)
        {
            if (ContainerVolume > DataVolume)
                return DataVolume;
            return ContainerVolume;
        }

        /// <inheritdoc/>
        public IHideResult Hide(string? data)
        {
            KochZhaoHideResult result = new();
            result.Log($"Запущен процесс скрытия для {Params.Image.ImgName}");

            // Начальные проверки
            if (data is not null)
                Params.Data = data;

            if (Params.Data.Length == 0)
            {
                result.Error("Отсутствуют данные для скрытия");
                return result;
            }
            result.Log($"Объём скрываемых данных: {Params.Data.Length} символов");

            // Доопределение параметров скрытия
            bool isRandomHiding = Params.Seed is not null;  // Вид скрытия: последовательный или псевдослучайный
            int containerVolume = GetContainerVolume();  // Объём контейнера с учётом числа НЗБ
            int hidingVolume = GetHidingVolume(containerVolume, Params.DataBitLength);  // Реальный объём скрытия
            double relativeHidingVolume = hidingVolume / containerVolume;  // Доля заполнения объёма контейнера
            int usingBlocksNum = Params.GetNeededToHideBlocksNum();  // Количество блоков, нужных для скрытия
            int blockSize = Params.GetBlockSize();  // Используемый размер блока

            // Логирование
            result.Log($"Установлены параметры: isRandomHiding = {isRandomHiding}, containerVolume = {containerVolume}, hidingVolume = {hidingVolume}, " +
                $"relativeHidingVolume = {relativeHidingVolume}, usingBlocksNum = {usingBlocksNum}, blockSize = {blockSize}");
            result.Log($"Для скрытия используется порог = {Params.Threshold}");

            // Выбор типа итерации в зависимости от метода скрытия (последовательное / псевдослучайное)
            Func<ImageBlocks, BlocksTraverseOptions, int?, IEnumerable<ScPointCoords>> iterator
                = isRandomHiding ? BlocksTraverseHelper.GetForRandomAccessIndexes : BlocksTraverseHelper.GetForLinearAccessIndexes;

            // Осуществление скрытия
            result.Log("Запущен цикл скрытия");
            int k = 0;  // Индекс бита данных
            ScPointCoords? firstblockIndex = null;
            ScPointCoords? lastblockIndex = null;

            var traversalOptions = new BlocksTraverseOptions(Params);
            foreach (var blockIndex in iterator(Params.ImgBlocks, traversalOptions, usingBlocksNum))
            {
                if (firstblockIndex is null)
                    firstblockIndex = blockIndex;
                if (k >= Params.DataBitLength)
                    break;

                bool bitToHide = Params.DataBitArray[k];  // Бит, который скрываем в блоке
                var block = BlocksTraverseHelper.GetBlockByIndexes(blockIndex, Params.ImgBlocks);
                var dctBlock = FrequencyViewTools.DctBlock(block, blockSize);  // Получение матрицы ДКП
                var newBlock = HideDataBitToDctBlock(dctBlock, bitToHide);  // Скрытие бита в блоке
                var idctBlock = FrequencyViewTools.IDctBlockAndNormalize(newBlock, blockSize);  // Обратное преобразование блока
                ChangeBlockInImageArray(idctBlock, blockIndex);

                k++;
                lastblockIndex = blockIndex;
            }
            result.Log("Завершён цикл скрытия");

            // Логирование
            if (!isRandomHiding && firstblockIndex is not null && lastblockIndex is not null)
                result.Log($"Скрытие осуществлено последовательно в блоки: с ({firstblockIndex.Value.Y}, {firstblockIndex.Value.X}, {firstblockIndex.Value.ChannelId}) по " +
                    $"({lastblockIndex.Value.Y}, {lastblockIndex.Value.X}, {lastblockIndex.Value.ChannelId})");
            result.Log($"В ходе скрытия должно быть записано {Params.DataBitLength} бит, реально записано {k} бит " +
                $"({(k == Params.DataBitLength ? "совпадает" : "не совпадает")})");

            // Сохранение изображения со внедрённой информацией
            string newImgName = Params.Image.ImgName + "_kz"
                + (isRandomHiding ? "_rnd" : "_lin");
            result.Path = Params.Image.Save(newImgName);
            result.Log($"Изображение сохранено как {newImgName}");

            result.Log($"Процесс скрытия завершён");
            return result;
        }

        /// <summary>
        /// Скрывает бит информации внутри блока
        /// </summary>
        private double[,] HideDataBitToDctBlock(double[,] dctBlock, bool bit)
        {
            var coefValues = FrequencyViewTools.GetBlockCoeffs(dctBlock, Params.HidingCoeffs);  // Значения коэффициентов
            var difference = MathMethods.GetModulesDiff(coefValues);  // Разница коэффициентов
            var newCoeffValues = coefValues;

            // Получение модифицированных значений коэффициентов
            if (bit == false && difference <= Params.Threshold)
                newCoeffValues = FrequencyViewTools.GetModifiedCoeffs(newCoeffValues, Params.Threshold, true);
            else if (bit == true && difference >= -Params.Threshold)
                newCoeffValues = FrequencyViewTools.GetModifiedCoeffs(newCoeffValues, -Params.Threshold, false);

            // Изменение значений на новые в блоке
            (int coefInd1, int coefInd2) = Params.HidingCoeffs.AsTuple();
            dctBlock[coefInd1, coefInd2] = newCoeffValues.val1;
            dctBlock[coefInd2, coefInd1] = newCoeffValues.val2;

            // Возвращает блок с модифицированными коэффициентами
            return dctBlock;
        }

        /// <summary>
        /// Записывает значения блока в массив пикселей изображения (одноканальный блок)
        /// </summary>
        private void ChangeBlockInImageArray(byte[,] block, ScPointCoords blockIndex)
        {
            // Определение границ блока
            var blockSize = Params.GetBlockSize();
            int maxY = blockIndex.Y + blockSize;
            int maxX = blockIndex.X + blockSize;

            // Запись в блок новых значений
            for (int y = blockIndex.Y; y < maxY; y++)
                for (int x = blockIndex.X; x < maxX; x++)
                    Params.Image.ImgArray[y, x, blockIndex.ChannelId] = block[y - blockIndex.Y, x - blockIndex.X];
        }
    }
}
