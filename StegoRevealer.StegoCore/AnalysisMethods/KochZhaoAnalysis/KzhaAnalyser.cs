﻿using System.Diagnostics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;
using StegoRevealer.StegoCore.StegoMethods;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;

namespace StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;

/// <summary>
/// Стегоанализатор метода Коха-Жао
/// </summary>
public class KzhaAnalyser
{
    private const string MethodName = "CKZhA (Consecutive Koch-Zhao Attack)";

    /// <summary>
    /// Параметры метода
    /// </summary>
    public KzhaParameters Params { get; set; }

    /// <summary>
    /// Внутренний метод-прослойка для записи в лог
    /// </summary>
    private Action<string> _writeToLog = (string str) => new string(str);


    public KzhaAnalyser(ImageHandler image)
    {
        Params = new KzhaParameters(image);
    }

    public KzhaAnalyser(KzhaParameters parameters)
    {
        Params = parameters;
    }


    /// <summary>
    /// Запуск стегоанализа
    /// </summary>
    /// <param name="verboseLog">Вести подробный лог</param>
    public KzhaResult Analyse(bool verboseLog = false)
    {
        var timer = Stopwatch.StartNew();

        var result = new KzhaResult();
        _writeToLog = result.Log;
        _writeToLog($"Started steganalysis by method '{MethodName}' for image '{Params.Image.ImgName}'");

        // Стегоанализ
        result = InnerAnalyse(result);

        // Попытка автоматического извлечения
        if (Params.TryToExtract && result.MessageBitsVolume > 0 && result.Threshold >= Params.Threshold)
        {
            string extractedData = string.Empty;

            var kzhaExtractionParams = GetKochZhaoParamsForAutoExtraction(result);
            var kzExtractor = new KochZhaoExtractor(kzhaExtractionParams);
            try
            {
                var extractResult = kzExtractor.Extract();
                if (extractResult is not null && extractResult.GetResultData() is not null)
                {
                    extractedData = extractResult.GetResultData() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _writeToLog($"Error while extracting: {ex.Message}");
            }

            result.ExtractedData = extractedData;  // Если включена попытка извлечения, будет string.Empty при неудаче
        }

        timer.Stop();
        _writeToLog($"Steganalysis by method '{MethodName}' ended for {timer.ElapsedMilliseconds} ms");

        result.ElapsedTime = timer.ElapsedMilliseconds;
        return result;
    }

    /// <summary>
    /// Основная логика метода стегоанализа
    /// </summary>
    private KzhaResult InnerAnalyse(KzhaResult result)
    {
        var cSequences = new Dictionary<ScIndexPair, List<double>>();
        foreach (var coeff in Params.AnalysisCoeffs)
            cSequences.Add(coeff, new List<double>());

        // Разбиение на блоки и установка параметров обхода
        var traversalOptions = GetTraversalOptions();
        var blocks = new ImageBlocks(new ImageBlocksParameters(Params.Image, Params.BlockSize));

        // Расчёт последовательности C
        var iterator = BlocksTraverseHelper.GetForLinearAccessOneChannelBlocksIndexes(blocks, traversalOptions);
        foreach (var blockIndex in iterator)
        {
            var blockCoords = blocks[blockIndex.Y, blockIndex.X];

            var dctBlock = FrequencyViewTools.DctBlock(Params.Image.ImgArray, blockCoords, blockIndex.ChannelId, Params.BlockSize);
            foreach (var coeff in Params.AnalysisCoeffs)
                cSequences[coeff].Add(GetAbsDiff(dctBlock, coeff));
        }
        
        // Создание массива задач
        var tasks = new Dictionary<ScIndexPair, Task<OneCoeffsPairAnalysisResult>>();
        foreach (var coeff in Params.AnalysisCoeffs)
            tasks[coeff] = Task.Run(() => AnalyseForOneCoeffPair(coeff, cSequences[coeff]));

        // Ожидание завершения задач стегоанализа
        foreach (var coeff in Params.AnalysisCoeffs)
            tasks[coeff].Wait();

        // Получение результатов
        var thresholds = new Dictionary<ScIndexPair, double>();  // Пороги подозрительных интервалов по наборам коэффициентов
        var indexes = new Dictionary<ScIndexPair, (int, int)?>();  // Индексы подозрительных интервалов по наборам коэффициентов
        var intervalsFounds = new Dictionary<ScIndexPair, bool>();

        foreach (var coeff in Params.AnalysisCoeffs)
        {
            var oneCoeffAnalysisResult = tasks[coeff].Result;
            thresholds[coeff] = oneCoeffAnalysisResult.Threshold;
            indexes[coeff] = oneCoeffAnalysisResult.Indexes.HasValue ? oneCoeffAnalysisResult.Indexes.Value.AsTuple() : null;
            intervalsFounds[coeff] = oneCoeffAnalysisResult.HasSuspiciousInterval;
        }

        // Выбор наибольшего по порогу из подозрительных интервалов в качестве результирующего
        var maxVariant = thresholds.FirstOrDefault(val => val.Value.Equals(thresholds.Values.Max())).Key;  // Ключ - набор коэффициентов
        result.SuspiciousInterval = indexes[maxVariant];
        result.Threshold = thresholds[maxVariant];
        result.Coefficients = maxVariant;

        result.MessageBitsVolume = thresholds[maxVariant] > Params.Threshold && result.SuspiciousInterval is not null
            ? result.SuspiciousInterval.Value.rightInd - result.SuspiciousInterval.Value.leftInd + 1
            : 0;

        // Считаем обнаружение состоявшимся, если преодолён минимальный порог (из параметров) и размер интервала больше 0
        if (result.Threshold >= Params.Threshold && result.MessageBitsVolume > 0)
            result.SuspiciousIntervalIsFound = true;

        _writeToLog($"The resulting coefficients are chosen: ({maxVariant.FirstIndex}, {maxVariant.SecondIndex})");
        _writeToLog($"Hided data (by evaluating): {(result.SuspiciousIntervalIsFound ? "exists" : "not exists")}");

        return result;
    }

    private OneCoeffsPairAnalysisResult AnalyseForOneCoeffPair(ScIndexPair coeff, List<double> cSequence)
    {
        // Логирование cSequences, если оно включено
        if (Params.LoggingCSequences)
        {
            _writeToLog($"Full cSequence for coefficients pair ({coeff.FirstIndex}, {coeff.SecondIndex}): [" +
                string.Join("; ", cSequence.Select(cValue => string.Format("{0:f2}", cValue))) + "]");
        }

        // Поиск ступенчатого интервала
        int intervalStartIndex = 0;

        // Получение непрерывного интервала аномально высоких значений cSequence
        (int indexLeft, int indexRight) = FindSuspiciousInterval(cSequence);
        _writeToLog($"For coefficients pair ({coeff.FirstIndex}, {coeff.SecondIndex}) calculated interval coordinates: [{indexLeft}:{indexRight}]");

        // Обрезка cSequences и сохранение оригинального индекса
        intervalStartIndex = indexLeft;  // Новый 0-й индекс в обрезанной cSequence на самом деле соответствует этому индексу последовательности
        cSequence = cSequence.GetRange(indexLeft, indexRight - indexLeft + 1);

        _writeToLog($"Truncated cSequence for coefficients pair ({coeff.FirstIndex}, {coeff.SecondIndex}): [" +
            string.Join("; ", cSequence.Select(cValue => string.Format("{0:f2}", cValue))) + "]");

        // Расчёт предполагаемого порога скрытия
        double threshold = 0.0;  // Порог подозрительного интервала по наборам коэффициентов
        ScIndexPair? indexes = null;  // Индекы подозрительного интервала по наборам коэффициентов
        bool hasSuspiciousInterval = false;

        bool detectedSecretData = cSequence.Count >= 8;  // Возможно ли извлечь хотя бы байт

        _writeToLog($"For coefficients pair ({coeff.FirstIndex}, {coeff.SecondIndex}) " +
            $"{(detectedSecretData ? "found suspicious interval" : "suspicious interval not found")}");

        // Запись подозрительного порога и интервала для текущего набора коэффициентов
        if (detectedSecretData)
        {
            threshold = cSequence.Min();  // Порог - минимальное из значений cSequence
            indexes = new ScIndexPair(intervalStartIndex, intervalStartIndex + cSequence.Count - 1);
            _writeToLog($"For coefficients pair ({coeff.FirstIndex}, {coeff.SecondIndex}) calculated analysis results: " +
                $"Threshold = {threshold}; Indexes = ({indexes?.FirstIndex}, {indexes?.SecondIndex})");
            hasSuspiciousInterval = true;  // Считаем, что подозрительный интервал (хотя бы один) найден
        }
        else
        {
            threshold = 0.0;
            indexes = null;
        }

        return new OneCoeffsPairAnalysisResult(threshold, indexes, hasSuspiciousInterval);
    }

    /// <summary>
    /// Установка параметров для поблочного обхода массива: эти параметры обусловлены непосредственно данным методом стегоанализа
    /// </summary>
    private BlocksTraverseOptions GetTraversalOptions()
    {
        var traverseOptions = new BlocksTraverseOptions();
        var startBlocks = new StartValues();
        traverseOptions.Channels.Clear();
        foreach (var channel in Params.Channels)
        {
            traverseOptions.Channels.Add(channel);
            startBlocks[channel] = 0;
        }
        traverseOptions.StartBlocks = startBlocks;
        traverseOptions.InterlaceChannels = false;
        traverseOptions.TraverseType = Params.TraverseType;

        return traverseOptions;
    }

    /// <summary>
    /// Установка параметров для попытки автоматического извлечения
    /// </summary>
    /// <param name="analysisResult">Результаты стегоанализа</param>
    private KochZhaoParameters GetKochZhaoParamsForAutoExtraction(KzhaResult analysisResult)
    {
        var kzParams = new KochZhaoParameters(Params.Image);
        var startBlocks = new StartValues();
        kzParams.Channels.Clear();
        foreach (var channel in Params.Channels)
        {
            kzParams.Channels.Add(channel);
            startBlocks[channel] = analysisResult.SuspiciousInterval?.leftInd ?? 0;
        }
        kzParams.StartBlocks = startBlocks;
        kzParams.InterlaceChannels = false;
        kzParams.TraverseType = Params.TraverseType;
        kzParams.HidingCoeffs = analysisResult.Coefficients;
        kzParams.ToExtractBitLength = analysisResult.MessageBitsVolume;
        kzParams.StegoOperation = StegoOperationType.Extracting;

        // Порог уменьшаем на 1 на всякий случай с учётом возможных погрешностей вычислений, иначе есть шанс "пропустить" блок из-за ошибок точности,
        // т.к. при анализе порог вычислен просто как наименьшее из значений cSequence
        kzParams.Threshold = analysisResult.Threshold > 1.0 ? analysisResult.Threshold - 1.0 : analysisResult.Threshold;

        return kzParams;
    }

    /// <summary>
    /// Считает модуль разницы модулей коэффициентов в блоке
    /// </summary>
    /// <param name="block">Блок</param>
    /// <param name="coeffs">Индексы коэффициентов блока</param>
    private static double GetAbsDiff(double[,] block, ScIndexPair coeffs)
    {
        (double value1, double value2) = (block[coeffs.FirstIndex, coeffs.SecondIndex], block[coeffs.SecondIndex, coeffs.FirstIndex]);
        return Math.Abs(MathMethods.GetModulesDiff(value1, value2));
    }

    /// <summary>
    /// Возвращает индексы "краёв" "подозрительной" последовательности - последовательности аномально высоких значений cSequence
    /// </summary>
    /// <param name="cSequence">Последовательность разниц модулей коэффициентов</param>
    private (int indexLeft, int indexRight) FindSuspiciousInterval(List<double> cSequence)
    {
        if (cSequence.Count < 2)
            throw new ArgumentException("Length of cSequence must be grather than 1");

        var maxValue = cSequence.Max();  // Определяем максимальное из значений cSequence
        var thresholdValue = maxValue * Params.CutCoefficient;  // Определяем минимальный порог превышения, который будет учитывать как подозрительный  // TODO

        // Отсечение всех значений меньше порога (приведение их к нулю)
        var truncatedSequence = cSequence.Select(c => c < thresholdValue ? 0.0 : c).ToList();

        (int resultLeftIndex, int resultRightIndex) = (0, 0);  // Левый и правый индексы финальной подозрительноый последовательности

        // Формирование набора интервалов непрерывных высоких значений (превысивших порог) - получение их координат
        var intervalIndexes = new List<(int leftIndex, int rightIndex)>();
        int currentLeftIndex = 0;
        int currentRightIndex = 0;
        for (int i = 0; i < truncatedSequence.Count; i++)
        {
            if (truncatedSequence[i].Equals(0.0))
            {
                currentRightIndex = i - 1;
                if (currentRightIndex - currentLeftIndex + 1 > 0)  // Если длина интервала при встрече "0" больше 0, то мы его записываем
                    intervalIndexes.Add((currentLeftIndex, currentRightIndex));
                currentLeftIndex = i + 1;
            }
        }

        // Выбор наиболее длинного интервала
        if (intervalIndexes.Count > 0)
        {
            var intervalSizes = intervalIndexes.Select(interval => interval.rightIndex - interval.leftIndex + 1).ToList();  // Длины интервалов
            var maxIntervalSize = intervalSizes.Max();  // Наибольшая длина интервала
            var maxInterval = intervalIndexes[intervalSizes.IndexOf(maxIntervalSize)];  // Получение первого по порядку интервала наибольшей длины
            (resultLeftIndex, resultRightIndex) = (maxInterval.leftIndex, maxInterval.rightIndex);
        }

        return (resultLeftIndex, resultRightIndex);  // Если интервалов не было найдено, вернётся (0,0)
    }

    /// <summary>
    /// Возвращает индексы двух последователных максимумов из последовательности (индекс левого из двух наибольших, индекс правого из двух наибольших)
    /// </summary>
    /// <param name="sequence">Последовательность (массив)</param>
    private static ScIndexPair GetTwoMaximumsIndexes(List<double> sequence)
    {
        if (sequence.Count < 2)
            throw new ArgumentException("Can't find two maximum values for sequence with length less than 2 elements");
        var thoMaximums = sequence.OrderByDescending(x => x).Take(2).ToList();  // Выбор двух максимальных значений

        // Поиск индексов двух максимумов
        var sequenceCopy = new List<double>(sequence);
        int leftIndex = sequence.IndexOf(thoMaximums[0]);
        sequenceCopy[leftIndex] = -1;  // В контексте метода все значения последовательности будут неотрицательными, "-1" "выключает" индекс из поиска второго максимума
        int rightIndex = sequence.IndexOf(thoMaximums[1]);

        // Важно вернуть первым левый из двух индексов, вторым - правый (независимо от того, по какому из них наибольшее из двух значений)
        return leftIndex > rightIndex ? new ScIndexPair(rightIndex, leftIndex) : new ScIndexPair(leftIndex, rightIndex);
    } 
}
