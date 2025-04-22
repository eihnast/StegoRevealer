using System.Diagnostics;
using System.Collections.Concurrent;
using StegoRevealer.StegoCore.CommonLib.Exceptions;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod;

/// <summary>
/// Стегоанализатор по методу Regular-Singular
/// </summary>
public class RsAnalyser
{
    private const string MethodName = "RS (Regular-Singular)";

    private static readonly object _lock = new object();

    /// <summary>
    /// Параметры метода
    /// </summary>
    public RsParameters Params { get; set; }

    /// <summary>
    /// Внутренний метод-прослойка для записи в лог
    /// </summary>
    private Action<string>? _writeToLog = null;


    public RsAnalyser(ImageHandler image)
    {
        Params = new RsParameters(image);
    }

    public RsAnalyser(RsParameters parameters)
    {
        Params = parameters;
    }


    /// <summary>
    /// Запуск стегоанализа
    /// </summary>
    /// <param name="verboseLog">Вести подробный лог</param>
    public RsResult Analyse(bool verboseLog = false)
    {
        var timer = Stopwatch.StartNew();

        var result = new RsResult();
        _writeToLog = result.Log;
        _writeToLog($"Started steganalysis by method '{MethodName}' for image '{Params.Image.ImgName}'");

        double pValuesSum = 0.0;  // Сумма P-значений по всем каналам (сумма относительных заполненностей, рассчитанных для каждого канала отдельно)

        var tasksByChannel = new Dictionary<ImgChannel, (Task<RsGroupsCalcResult> UnturnedTask, Task<RsGroupsCalcResult> InvertedTask)>();
        foreach (var channel in Params.Channels)
        {
            var unturnedCalcTask = Task.Run(() => AnalyseInOneChannel(channel, invertedImage: false));
            var invertedCalsTask = Task.Run(() => AnalyseInOneChannel(channel, invertedImage: true));
            tasksByChannel.Add(channel, (unturnedCalcTask, invertedCalsTask));
        }

        foreach (var calcTasks in tasksByChannel.Values)
        {
            calcTasks.UnturnedTask.Wait();
            calcTasks.InvertedTask.Wait();
        }

        foreach (var channel in Params.Channels)
        {
            var unturnedValues = tasksByChannel[channel].UnturnedTask.Result;
            _writeToLog($"Calculations for {channel} channel in original image completed. Regulars = {unturnedValues.Regulars}, Singulars = {unturnedValues.Singulars}, " +
                $"Regulars (inverted mask) = {unturnedValues.RegularsWithInvertedMask}, Singulars (inverted mask) = {unturnedValues.SingularsWithInvertedMask}");

            var invertedValues = tasksByChannel[channel].InvertedTask.Result;
            _writeToLog($"Calculations for {channel} channel in original image completed. Regulars = {invertedValues.Regulars}, Singulars = {invertedValues.Singulars}, " +
                $"Regulars (inverted mask) = {invertedValues.RegularsWithInvertedMask}, Singulars (inverted mask) = {invertedValues.SingularsWithInvertedMask}");

            var pValue = CalculatePValue(unturnedValues, invertedValues);
            pValuesSum += pValue;

            _writeToLog($"Relative message volume at channel '{channel}' (pValue): {pValue}");
            result.MessageRelativeVolumesByChannels[channel] = pValue;
        }

        result.MessageRelativeVolume = pValuesSum / Params.Channels.Count;
        _writeToLog($"Average relative message volume = {result.MessageRelativeVolume}");

        timer.Stop();
        _writeToLog($"Steganalysis by method '{MethodName}' ended for {timer.ElapsedMilliseconds} ms");

        result.ElapsedTime = timer.ElapsedMilliseconds;
        return result;
    }

    /// <summary>
    /// Метод выполнения одной итерации анализа для выбранного цветового канала
    /// </summary>
    /// <param name="channel">Цветовой канал</param>
    /// <param name="invertedImage">Вести ли подсчёт в изображении с "инвертированными" НЗБ</param>
    /// <returns>Количество каждой из групп</returns>
    private RsGroupsCalcResult AnalyseInOneChannel(ImgChannel channel, bool invertedImage = false)
    {
        var result = new RsGroupsCalcResult();
        var channelArray = invertedImage ? Params.Image.Inverted.ChannelsArray.GetChannelArray(channel) : Params.Image.ChannelsArray.GetChannelArray(channel);
        if (channelArray is null)
            throw new ArgumentException($"Error while getting OneChannelArray for channel '{channel}'");

        //var groups = SplitIntoGroupsInChannelArray(channelArray);  // Дешевле хранить массивы по 4 byte-значения группы, чем координаты группы (минимум 4 int на группу)
        //result.GroupsNumber = groups.Count;

        int blockNumber = 0;

        var traversalOptions = Params.GetTraversalOptions();
        traversalOptions.Channels = [channel];
        var iterator = BlocksTraverseHelper.GetForLinearAccessBlocksIndexes(Params.ImgBlocks, traversalOptions);

        var flippingFuncs = GetFlippingFuncsByMask(Params.FlippingMask);
        var flippingFuncsWithInvertedMask = GetFlippingFuncsByMask(RsHelper.InvertMask(Params.FlippingMask));

        Parallel.ForEach(iterator,
            () => new RsGroupsCalcResult(),
            (block, state, localResult) =>
            {
                var pixels = GetLinearPixelsList(block, channelArray);

                var regularityResult = CalculateRegularityResults(pixels, flippingFuncs);
                var groupType = RsHelper.DefineGroupType(regularityResult);
                switch (groupType)
                {
                    case RsGroupType.Singular:
                        localResult.Singulars++;
                        break;
                    case RsGroupType.Regular:
                        localResult.Regulars++;
                        break;
                }

                var regularityWithInvertedMaskResult = CalculateRegularityResults(pixels, flippingFuncsWithInvertedMask);
                var groupTypeWithInvertedMask = RsHelper.DefineGroupType(regularityWithInvertedMaskResult);
                switch (groupTypeWithInvertedMask)
                {
                    case RsGroupType.Singular:
                        localResult.SingularsWithInvertedMask++;
                        break;
                    case RsGroupType.Regular:
                        localResult.RegularsWithInvertedMask++;
                        break;
                }

                Interlocked.Increment(ref blockNumber);
                return localResult;
            },
            localResult =>
            {
                lock (_lock)
                {
                    result.Singulars += localResult.Singulars;
                    result.Regulars += localResult.Regulars;
                    result.SingularsWithInvertedMask += localResult.SingularsWithInvertedMask;
                    result.RegularsWithInvertedMask += localResult.RegularsWithInvertedMask;
                }
            }
        );

        result.GroupsNumber = blockNumber + 1;
        return result;
    }

    /// <summary>
    /// Метод расчёта оценки заполненности контейнера на основе расчётов по RS-диаграмме
    /// </summary>
    /// <param name="unturnedValues">Значения в "обычном" изображении</param>
    /// <param name="invertedValues">Значения в изображении с "инвертированными" НЗБ</param>
    /// <returns></returns>
    private double CalculatePValue(RsGroupsCalcResult unturnedValues, RsGroupsCalcResult invertedValues)
    {
        if (unturnedValues.GroupsNumber != invertedValues.GroupsNumber)
            throw new CalculationException("Error while RS groups calculating: unturnedValues and invertedValues is not equals");

        double Rm = (double)unturnedValues.Regulars / unturnedValues.GroupsNumber;
        double Sm = (double)unturnedValues.Singulars / unturnedValues.GroupsNumber;
        double Rmi = (double)unturnedValues.RegularsWithInvertedMask / unturnedValues.GroupsNumber;
        double Smi = (double)unturnedValues.SingularsWithInvertedMask / unturnedValues.GroupsNumber;
        double invRm = (double)invertedValues.Regulars / invertedValues.GroupsNumber;
        double invSm = (double)invertedValues.Singulars / invertedValues.GroupsNumber;
        double invRmi = (double)invertedValues.RegularsWithInvertedMask / invertedValues.GroupsNumber;
        double invSmi = (double)invertedValues.SingularsWithInvertedMask / invertedValues.GroupsNumber;

        _writeToLog?.Invoke($"RS-diagram points values: Rm = {Rm:0.###}; Sm = {Sm:0.###}; Rmi = {Rmi:0.###}; Smi = {Smi:0.###}; " +
            $"invRm = {invRm:0.###}; invSm = {invSm:0.###}; invRmi = {invRmi:0.###}; invSmi = {invSmi:0.###}");

        // Mathematical code
        double d0 = Rm - Sm;
        double d0i = Rmi - Smi;
        double d1 = invRm - invSm;
        double d1i = invRmi - invSmi;

        double a = (d1 + d0) * 2;
        double b = d0i - d1i - d1 - 3 * d0;
        double c = d0 - d0i;

        double D = Math.Pow(b, 2) - 4 * a * c;

        _writeToLog?.Invoke($"Intermediate values: d0 = {d0:0.###}; d0i = {d0i:0.###}; d1 = {d1:0.###}; d1i = {d1i:0.###}; " +
            $"a = {a:0.###}; b = {b:0.###}; c = {c:0.###}. Discriminant D = {D:0.####}");

        double x1 = 0, x2 = 0, minX = 0;
        if (D.Equals(0.0))
            x1 = x2 = minX = -(b / 2 * a);
        if (D > 0.0)
        {
            x1 = (-b + Math.Sqrt(D)) / (2 * a);
            x2 = (-b - Math.Sqrt(D)) / (2 * a);
            if (Math.Abs(x1) < Math.Abs(x2))
                minX = x1;
            else
                minX = x2;
        }

        _writeToLog?.Invoke($"Roots: x1 = {x1:0.###}; x2 = {x2:0.###}. Minimum root value = {minX:0.###}");

        double p = minX / (minX - 0.5);
        return Math.Max(p, 0.0);
    }


    /// <summary>
    /// Метод формирования массива групп (разбиения изображения на группы)
    /// </summary>
    /// <param name="channelArray">Ссылка на массив пикселей изображения в одном канале</param>
    [Obsolete("Вместо старого разбиения на группы необходимо использовать общий механизм итерации по блокам")]
    private List<int[]> SplitIntoGroupsInChannelArray(ImageOneChannelArray channelArray)
    {
        var groups = new ConcurrentBag<int[]>();
        Parallel.For(0, channelArray.Height, y =>
        {
            for (int xGroup = 0; xGroup < channelArray.Width / Params.PixelsGroupLength; xGroup++)
            {
                var group = new int[Params.PixelsGroupLength];
                for (int i = 0; i < Params.PixelsGroupLength; i++)
                {
                    int x = xGroup * Params.PixelsGroupLength + i;
                    if (x >= channelArray.Width)
                        throw new CalculationException("Index out of bounds in channelArray.");
                    group[i] = channelArray[y, x];
                }
                groups.Add(group);
            }

            // "Лишние" пиксели, которые не попадают в последовательную выборку групп, в оригинале учитывались
            // и собирались в дополнительные "остаточные" группы здесь (в единый массив excess, откуда по 
            // достижению длины PixelsGroupLength сразу же выгружались в основной).
        });

        return groups.ToList();
    }

    private IEnumerable<int> GetLinearPixelsList(Sc2DPoint blockCoords, ImageOneChannelArray channelArray)
    {
        var blockPixelsIndexes = Params.ImgBlocks[blockCoords.Y, blockCoords.X];
        var pixels = new List<int>();

        for (int y = blockPixelsIndexes.Lt.Y; y <= blockPixelsIndexes.Rd.Y; y++)
        {
            for (int x = blockPixelsIndexes.Lt.X; x <= blockPixelsIndexes.Rd.X; x++)
            {
                var pixel = channelArray[y, x];
                pixels.Add(pixel);
            }
        }

        return pixels;
    }

    /// <summary>
    /// Метод формирования списка функций флиппинга для применения их по маске к группе пикселей
    /// </summary>
    /// <param name="mask">Маска</param>
    /// <returns>Список функций, которые должны быть применены к значениям группы</returns>
    private Func<int, int>[] GetFlippingFuncsByMask(int[] mask)
    {
        var funcs = new List<Func<int, int>>();
        foreach (int maskValue in mask)
        {
            switch (maskValue)
            {
                case 1:
                    funcs.Add(Params.FlipDirect);
                    break;
                case 0:
                    funcs.Add(Params.FlipNone);
                    break;
                case -1:
                    funcs.Add(Params.FlipBack);
                    break;
                default:
                    throw new ArgumentException("Mask value must be -1, 0 or 1");
            }
        }

        return funcs.ToArray();
    }

    /// <summary>
    /// Расчёт значений регулярности для группы
    /// </summary>
    /// <param name="group">Группа</param>
    /// <param name="flippingFuncs">Массив функций для применения к значениям группы по маске</param>
    /// <returns>Значения регулярности до и после флиппинга</returns>
    private (int beforeFlippingResult, int afterFlippingResult) CalculateRegularityResults(IEnumerable<int> group, Func<int, int>[] flippingFuncs)
    {
        var flippedGroup = RsHelper.ApplyFlipping(group, flippingFuncs);  // Расчёт "перевёрнутой" группы
        int beforeFlippingResult = Params.RegularityFunction(group);
        int afterFlippingResult = Params.RegularityFunction(flippedGroup);
        return (beforeFlippingResult, afterFlippingResult);
    }

}
