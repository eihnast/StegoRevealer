using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Collections;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    public class KochZhaoHider : IHider
    {
        public KochZhaoParameters Params { get; set; }

        public KochZhaoHider(ImageHandler img)
        {
            Params = new KochZhaoParameters(img);
        }

        // Допустимый объём контейнера
        private int GetContainerVolume()
        {
            var blocksNum = Params.GetAllBlocksNum();
            return blocksNum;
        }

        // Определение скрываемого объёма информации (учитывает допустимый объём контейнера)
        private static int GetHidingVolume(int ContainerVolume, int DataVolume)
        {
            if (ContainerVolume > DataVolume)
                return DataVolume;
            return ContainerVolume;
        }

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
            result.Log($"Установлены параметры:\n\t" +
                $"isRandomHiding = {isRandomHiding}\n\tcontainerVolume = {containerVolume}\n\t" +
                $"hidingVolume = {hidingVolume}\n\trelativeHidingVolume = {relativeHidingVolume}\n\t" +
                $"usingBlocksNum = {usingBlocksNum}\n\tblockSize = {blockSize}");

            // Выбор типа итерации в зависимости от метода скрытия (последовательное / псевдослучайное)
            Func<KochZhaoParameters, int?, IEnumerable<(int y, int x, int ch)>> iterator
                = isRandomHiding ? KochZhaoCommon.GetForRandomAccessIndex : KochZhaoCommon.GetForLinearAccessIndex;

            // Осуществление скрытия
            result.Log("Запущен цикл скрытия");
            int k = 0;  // Индекс бита данных
            foreach (var blockIndex in iterator(Params, usingBlocksNum))
            {
                if (k >= Params.DataBitLength)
                    break;

                bool bitToHide = Params.DataBitArray[k];  // Бит, который скрываем в блоке
                var block = KochZhaoCommon.GetBlockByIndex(blockIndex.y, blockIndex.x, blockIndex.ch, Params);
                var dctBlock = KochZhaoCommon.DctBlock(block, blockSize);  // Получение матрицы ДКП
                var newBlock = HideDataBitToDctBlock(dctBlock, bitToHide);  // Скрытие бита в блоке
                var idctBlock = KochZhaoCommon.IDctBlockAndNormalize(newBlock, blockSize);  // Обратное преобразование блока
                ChangeBlockInImageArray(idctBlock, blockIndex);

                k++;
            }
            result.Log("Завершён цикл скрытия");

            // Сохранение изображения со внедрённой информацией
            string newImgName = Params.Image.ImgName + "_kz"
                + (isRandomHiding ? "_rnd" : "_lin");
            result.Path = Params.Image.Save(newImgName);
            result.Log($"Изображение сохранено как {newImgName}");

            result.Log($"Процесс скрытия завершён");
            return result;
        }

        // Скрывает бит информации внутри блока
        private double[,] HideDataBitToDctBlock(double[,] dctBlock, bool bit)
        {
            var coefValues = KochZhaoCommon.GetBlockCoeffs(dctBlock, Params.HidingCoeffs);  // Значения коэффициентов
            var difference = MathMethods.GetModulesDiff(coefValues);  // Разница коэффициентов
            var newCoeffValues = coefValues;

            // Получение модифицированных значений коэффициентов
            if (bit == false && difference <= Params.Threshold)
                newCoeffValues = KochZhaoCommon.GetModifiedCoeffs(newCoeffValues, Params.Threshold, true);
            else if (bit == true && difference >= -Params.Threshold)
                newCoeffValues = KochZhaoCommon.GetModifiedCoeffs(newCoeffValues, -Params.Threshold, false);

            // Изменение значений на новые в блоке
            (int coefInd1, int coefInd2) = Params.HidingCoeffs;
            dctBlock[coefInd1, coefInd2] = newCoeffValues.val1;
            dctBlock[coefInd2, coefInd1] = newCoeffValues.val2;

            // Возвращает блок с модифицированными коэффициентами
            return dctBlock;
        }

        // Записывает значения блока в массив пикселей изображения (одноканальный блок)
        private void ChangeBlockInImageArray(byte[,] block, (int y, int x, int ch) blockIndex)
        {
            // Определение границ блока
            var blockSize = Params.GetBlockSize();
            int maxY = blockIndex.y + blockSize;
            int maxX = blockIndex.x + blockSize;

            // Запись в блок новых значений
            for (int y = blockIndex.y; y < maxY; y++)
                for (int x = blockIndex.x; x < maxX; x++)
                    Params.Image.ImgArray[y, x, blockIndex.ch] = block[y - blockIndex.y, x - blockIndex.x];
        }
    }
}
