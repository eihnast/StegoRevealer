using Accord.Math.Optimization;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;
using System.Collections;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao;

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
    public IHideResult Hide(string? data, string? newImagePath = null) => HideAlgorithm(data, newImagePath);

    /// <inheritdoc/>
    public IHideResult Hide(IParams parameters, string? data, string? newImagePath = null)
    {
        KochZhaoHideResult result = new();
        
        KochZhaoParameters? kzParams = parameters as KochZhaoParameters;
        if (kzParams is null)  // Не удалось привести к KochZhaoParameters
        {
            result.Error("kzParams является null");
            return result;
        }

        // Замена параметров на переданные
        var oldKzParams = Params;
        Params = kzParams;

        result = HideAlgorithm(data, newImagePath);
        Params = oldKzParams;  // Возврат параметров
        return result;
    }

    // Логика метода с текущими параметрами
    private KochZhaoHideResult HideAlgorithm(string? data, string? newImagePath)
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
        int containerVolume = GetContainerVolume();  // Объём контейнера с учётом числа блоков
        int hidingVolume = GetHidingVolume(containerVolume, Params.DataBitLength);  // Реальный объём скрытия
        double relativeHidingVolume = (double)hidingVolume / containerVolume;  // Доля заполнения объёма контейнера
        int usingBlocksNum = Params.GetNeededBlocksNum();  // Количество блоков, нужных для скрытия
        int blockSize = Params.BlockSize;  // Используемый размер блока

        // Логирование
        result.Log($"Установлены параметры: isRandomHiding = {isRandomHiding}, containerVolume = {containerVolume}, hidingVolume = {hidingVolume}, " +
            $"relativeHidingVolume = {relativeHidingVolume}, usingBlocksNum = {usingBlocksNum}, blockSize = {blockSize}");
        result.Log($"Для скрытия используется порог = {Params.Threshold}");

        // Выбор типа итерации в зависимости от метода скрытия (последовательное / псевдослучайное)
        Func<ImageBlocks, BlocksTraverseOptions, int?, IEnumerable<ScPointCoords>> iterator
            = isRandomHiding ? BlocksTraverseHelper.GetForRandomAccessOneChannelBlocksIndexes : BlocksTraverseHelper.GetForLinearAccessOneChannelBlocksIndexes;

        // Осуществление скрытия
        result.Log("Запущен цикл скрытия");
        int k = 0;  // Индекс бита данных
        ScPointCoords? firstblockIndex = null;
        ScPointCoords? lastblockIndex = null;

        // Параметры обхода применяются для получения конкретного итератора и не хранятся как часть общих параметров метода
        var traversalOptions = new BlocksTraverseOptions(Params);

        var blockForDataIndexArray = new List<KeyValuePair<int, ScPointCoords>>();
        foreach (var blockIndex in iterator(Params.ImgBlocks, traversalOptions, usingBlocksNum))
        {
            if (firstblockIndex is null)
                firstblockIndex = blockIndex;
            if (k >= Params.DataBitLength)
                break;

            blockForDataIndexArray.Add(new KeyValuePair<int, ScPointCoords>(k, blockIndex));

            k++;
            lastblockIndex = blockIndex;
        }

        int basketsCount = 3;
        int basketSize = blockForDataIndexArray.Count / basketsCount;
        var baskets = new List<List<KeyValuePair<int, ScPointCoords>>>();
        for (int i = 0; i < basketsCount - 1; i++)
            baskets.Add(blockForDataIndexArray.Take((basketSize * i)..(basketSize * (i + 1))).ToList());
        baskets.Add(blockForDataIndexArray.Take((basketSize * (basketsCount - 1))..blockForDataIndexArray.Count).ToList());

        var basketTasks = new List<Task>();
        foreach (var basket in baskets)
        {
            basketTasks.Add(new Task(() =>
            {
                foreach (var blockForDataIndex in basket)
                {
                    bool bitToHide = Params.DataBitArray[blockForDataIndex.Key];  // Бит, который скрываем в блоке
                    var block = BlocksTraverseHelper.GetOneChannelBlockByIndexes(blockForDataIndex.Value, Params.ImgBlocks);
                    var dctBlock = FrequencyViewTools.DctBlock(block, blockSize);  // Получение матрицы ДКП
                    var newBlock = HideDataBitToDctBlock(dctBlock, bitToHide);  // Скрытие бита в блоке
                    var idctBlock = FrequencyViewTools.IDctBlockAndNormalize(newBlock, blockSize);  // Обратное преобразование блока
                    ChangeBlockInImageArray(idctBlock, blockForDataIndex.Value);
                }
            }));
        }

        foreach (var basketTask in basketTasks)
            basketTask.Start();
        foreach (var basketTask in basketTasks)
            basketTask.Wait();
        
        result.Log("Завершён цикл скрытия");

        // Логирование
        if (!isRandomHiding && firstblockIndex is not null && lastblockIndex is not null)
            result.Log($"Скрытие осуществлено последовательно в блоки: с ({firstblockIndex.Value.Y}, {firstblockIndex.Value.X}, {firstblockIndex.Value.ChannelId}) по " +
                $"({lastblockIndex.Value.Y}, {lastblockIndex.Value.X}, {lastblockIndex.Value.ChannelId})");
        result.Log($"В ходе скрытия должно быть записано {Params.DataBitLength} бит, реально записано {k} бит " +
            $"({(k == Params.DataBitLength ? "совпадает" : "не совпадает")})");

        // Сохранение изображения со внедрённой информацией
        if (string.IsNullOrEmpty(newImagePath))
        {
            string newImageName = Params.Image.ImgName + "_kz" + (isRandomHiding ? "_rnd" : "_lin");
            result.Path = Params.Image.SaveNear(newImageName);
        }
        else
        {
            result.Path = Params.Image.Save(newImagePath);
        }
        result.Log($"Изображение сохранено как {result.Path}");

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
        var blockSize = Params.BlockSize;
        int blockStartY = blockIndex.Y * blockSize;
        int blockStartX = blockIndex.X * blockSize;
        int blockEndY = blockStartY + blockSize - 1;
        int blockEndX = blockStartX + blockSize - 1;

        // Запись в блок новых значений
        for (int y = blockStartY; y < blockEndY; y++)
            for (int x = blockStartX; x < blockEndX; x++)
                Params.Image.ImgArray[y, x, blockIndex.ChannelId] = block[y - blockStartY, x - blockStartX];
    }
}
