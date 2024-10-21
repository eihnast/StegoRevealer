using Accord.Math;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;

namespace StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;

/// <summary>
/// Стегоанализатор по методу Хи-квадрат
/// </summary>
public class ChiSquareAnalyser
{
    private const string MethodName = "Pair analysis (Chi-Square)";

    /// <summary>
    /// Параметры метода
    /// </summary>
    public ChiSquareParameters Params { get; set; }

    // Список координат блоков (кортежи: левые верхние и правые нижние координаты), заполняется при первом проходе по массиву пикселей
    // private List<(Sc2DPoint lt, Sc2DPoint rd)>? _lazyBlocksCoordsList = null;


    public ChiSquareAnalyser(ImageHandler image)
    {
        Params = new ChiSquareParameters(image);
    }

    public ChiSquareAnalyser(ChiSquareParameters parameters)
    {
        Params = parameters;
    }


    /// <summary>
    /// Запуск стегоанализа
    /// </summary>
    /// <param name="verboseLog">Вести подробный лог</param>
    public ChiSquareResult Analyse(bool verboseLog = false)
    {
        var result = new ChiSquareResult();
        result.Log($"Выполняется стегоанализ методом {MethodName} для файла изображения {Params.Image.ImgName}");

        var cnumArr = new int[Params.UseUnitedCnum ? 256 : Params.Channels.Count * 256];
        cnumArr = Enumerable.Repeat(Params.UseIncreasedCnum ? 1 : 0, cnumArr.Length).ToArray();

        double fullness = 0.0;  // Относительная заполненность контейнера
        int blockNumber = 0;  // Счётчик блоков

        var toColorizeChannels = new List<ImgChannel>();
        var traversalOptions = Params.GetTraversalOptions();
        var iterator = BlocksTraverseHelper.GetForLinearAccessBlocksIndexes(Params.ImgBlocks, traversalOptions);

        foreach (var block in iterator)
        {
            var blockPixelsIndexes = Params.ImgBlocks[block.Y, block.X];

            // Формирование массива количеств цветов
            if (Params.UsePreviousCnums)
                AddCnumArrays(cnumArr, CreateCnumArr(blockPixelsIndexes));
            else
                cnumArr = CreateCnumArr(blockPixelsIndexes);

            // Создание массивов наблюдаемых и ожидаемых величин
            var (expected, observed) = CreateChiArrays(cnumArr);

            // Объединение низкочастотных категорий, если задана настройка
            if (Params.UseUnifiedCathegories && expected.Count >= 2)
                (expected, observed) = UnifyCathegories(expected, observed);

            // Вычисление результатов оценки
            var chiSqr = MathMethods.ChiSqr(expected, observed);
            var blockContainsHiddenInfo = chiSqr.pValue > Params.Threshold;
            if (blockContainsHiddenInfo)
                fullness += 1;  // +1 блок со встроенной информацией

            // Если необходима - визуализация скрытия в блоке: запись нужного канала для блока
            if (Params.Visualize)
                toColorizeChannels.Add(blockContainsHiddenInfo ? ImgChannel.Red : ImgChannel.Green);

            blockNumber++;

            // Подробное логирование
            if (verboseLog)
            {
                int pixelsNum = (blockPixelsIndexes.Rd.Y - blockPixelsIndexes.Lt.Y + 1) * (blockPixelsIndexes.Rd.X - blockPixelsIndexes.Lt.X + 1);
                result.Log($"Блок № {blockNumber}");
                result.Log($"\tБлок содержит {pixelsNum} пикселей");
                result.Log(string.Format("\tChi-Square: {0:f2}\tP-Value: {1:f2}", chiSqr.statistic, chiSqr.pValue));
                result.Log("");
            }
        }

        fullness /= blockNumber;  // Делим количество заполненных блоков на количество всех блоков
        result.MessageRelativeVolume = fullness;  // Относительный объём скрытого сообщения

        // Визуализация скрытия на изображении целиком
        if (Params.Visualize)
            result.Image = ColorizeAllImage(toColorizeChannels, 100);

        result.Log($"Стегоанализ методом {MethodName} завершён");
        return result;
    }

    /// <summary>
    /// Визуализирует скрытие во всём изображении поблочно
    /// </summary>
    /// <param name="channelsToColorizeInBlock">Массив каналов, в которые надо сместить цвет для каждого блока</param>
    /// <param name="colorOffset">Смещение цвета</param>
    private ImageHandler ColorizeAllImage(List<ImgChannel> channelsToColorizeInBlock, int colorOffset = 100)
    {
        var colorizedImage = Params.Image.Clone();

        // Используется обход координат - можно запускать обход старого изображения
        var traversalOptions = Params.GetTraversalOptions();
        var iterator = BlocksTraverseHelper.GetForLinearAccessBlocksIndexes(Params.ImgBlocks, traversalOptions);

        int i = 0;
        foreach (var blockIndexes in iterator)
        {
            var coords = Params.ImgBlocks[blockIndexes.Y, blockIndexes.X];
            var channelId = (int)channelsToColorizeInBlock[i];

            for (int y = coords.Lt.Y; y <= coords.Rd.Y; y++)
            {
                for (int x = coords.Lt.X; x <= coords.Rd.X; x++)
                {
                    var colorByte = colorizedImage.ImgArray[y, x, channelId];
                    var newValue = Convert.ToByte(Math.Min((int)colorByte + colorOffset, 255));
                    colorizedImage.ImgArray[y, x, channelId] = newValue;
                }
            }

            i++;
        }

        return colorizedImage;
    }

    /// <summary>
    /// Объединяет низкочастотные категории
    /// </summary>
    /// <param name="oldExpected">Список ожидаемых значений категорий</param>
    /// <param name="oldObserved">Список наблюдаемых значений категорий</param>
    /// <returns>Новые списки ожидаемых и наблюдаемых значений</returns>
    /// <exception cref="ArgumentException">Размеры список ожидаемых и наблюдаемых значений не совпадают</exception>
    private (List<double> expected, List<double> observed) UnifyCathegories(List<double> oldExpected, List<double> oldObserved)
    {
        if (oldExpected.Count != oldObserved.Count)
            throw new ArgumentException("Sizes of arrays oldExpected and oldObserved is not equal");

        var newExpected = new List<double>();
        var newObserved = new List<double>();
        var toUnifyExpected = new List<double>();
        var toUnifyObserved = new List<double>();

        // Отбор подходящих по порогу категорий
        for (int i = 0; i < oldExpected.Count; i++)
        {
            var expectedValue = oldExpected[i];
            if (expectedValue > Params.UnifyingCathegoriesThreshold)
            {
                newExpected.Add(expectedValue);
                newObserved.Add(oldObserved[i]);
            }
            else
            {
                toUnifyExpected.Add(expectedValue);
                toUnifyObserved.Add(oldObserved[i]);
            }
        }

        // Цикл объединения категорий
        bool unified = false;
        while (!unified)
        {
            unified = true;  // По умолчанию считаем новую итерацию последней

            // Объединяем соседние категории
            for (int i = 0; i < toUnifyExpected.Count / 2; i++)
            {
                toUnifyExpected[2 * i] += toUnifyExpected[2 * i + 1];
                toUnifyExpected[2 * i + 1] = -1;
                toUnifyObserved[2 * i] += toUnifyObserved[2 * i + 1];
                toUnifyObserved[2 * i + 1] = -1;
            }

            // Переносим достаточно высокочастотные категории в основной массив
            for (int i = 0; i < toUnifyExpected.Count; i++)
                if (toUnifyExpected[i] > Params.UnifyingCathegoriesThreshold)
                {
                    newExpected.Add(toUnifyExpected[i]);
                    toUnifyExpected[i] = -1;
                    newObserved.Add(toUnifyObserved[i]);
                    toUnifyObserved[i] = -1;
                }

            // Удаляем из массивов объединений удалённые и перенесённые элементы
            toUnifyExpected.RemoveAll(val => val == -1);
            toUnifyObserved.RemoveAll(val => val == -1);

            // Решаем проблему последней категории
            if (toUnifyExpected.Count == 1)
            {
                if (toUnifyExpected[0] > Params.UnifyingCathegoriesThreshold)  // Если удовлетворяет порогу
                {
                    newExpected.Add(toUnifyExpected[0]);
                    newObserved.Add(toUnifyObserved[0]);
                }
                else  // Если последняя категория не удовлетворяет порогу
                {
                    var unifingCatIndex = newExpected.IndexOf(newExpected.Min());  // Минимальная категория
                    newExpected[unifingCatIndex] += toUnifyExpected[0];
                    newObserved[unifingCatIndex] += toUnifyObserved[0];
                }
            }
            else if (toUnifyExpected.Count > 0)
                unified = false;  // Категорий больше 0 и она и 1 => продолжаем объединение
        }

        return (newExpected, newObserved);
    }

    /// <summary>
    /// Формирует массивы ожидаемых и наблюдаемых значений
    /// </summary>
    /// <param name="cnum">Массив количества появлений цветов</param>
    /// <returns>Списки ожидаемых и наблюдаемых значений категорий</returns>
    private (List<double> expected, List<double> observed) CreateChiArrays(int[] cnum)
    {
        int arraysLength = cnum.Length / 2;
        var expected = new List<double>();
        var observed = new List<double>();

        for (int i = 0; i < arraysLength; i++)
        {
            double expectedValue = (double)(cnum[2 * i] + cnum[2 * i + 1]) / 2;
            double observedValue = (double)cnum[2 * i];

            if (expectedValue > 0 || !Params.ExcludeZeroPairs)
            {
                expected.Add(expectedValue);
                observed.Add(observedValue);
            }
        }

        return (expected, observed);
    }

    // Добавляет к первому массиву CnumArr значения второго
    private static void AddCnumArrays(int[] mainArray, int[] newArray)
    {
        if (mainArray.Length != newArray.Length)
            throw new ArgumentException("Sizes of mainArray and newArray for adding Cnum is not equal");

        for (int i = 0; i < newArray.Length; i++)
            mainArray[i] += newArray[i];
    }

    /// <summary>
    /// Формирует CnumArr - ColorsNumArray, массив количества интенсивности цветов пикселей (массив количества появлений цветов)
    /// </summary>
    /// <param name="pixels">Одномерный массив всех пикселей</param>
    /// <returns>Массив количества появлений цветов</returns>
    private int[] CreateCnumArr(BlockCoords blockCoords)
    {
        var imar = Params.Image.ImgArray;
        if (Params.UseUnitedCnum)  // Если считаем только значения интенсивности без учёта канала
        {
            var cnum = Enumerable.Repeat(0, 256).ToArray();
            
            for (int y = blockCoords.Lt.Y; y <= blockCoords.Rd.Y; y++)
            {
                for (int x = blockCoords.Lt.X; x <= blockCoords.Rd.X; x++)
                {
                    var pixel = imar[y, x];
                    foreach (var channel in Params.Channels)
                    {
                        var channelIndex = (int)channel;
                        var colorByte = pixel[channelIndex];
                        cnum[colorByte]++;
                    }
                }
            }
            return cnum;
        }
        else  // Если считаем количество значений интенсивности для каждого канала отдельно
        {
            var cnum = Enumerable.Repeat(0, Params.Channels.Count * 256).ToArray();
            for (int y = blockCoords.Lt.Y; y < blockCoords.Rd.Y; y++)
            {
                for (int x = blockCoords.Lt.X; x < blockCoords.Rd.X; x++)
                {
                    var pixel = imar[y, x];
                    for (int ch = 0; ch < Params.Channels.Count; ch++)
                    {
                        var channel = Params.Channels[ch];
                        var channelIndex = (int)channel;
                        var colorByte = pixel[channelIndex];
                        cnum[ch * 256 + colorByte]++;
                    }
                }
            }
            return cnum;
        }
    }
}
