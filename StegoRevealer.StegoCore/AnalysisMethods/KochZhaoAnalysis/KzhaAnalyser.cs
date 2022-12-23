using System.Globalization;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;
using StegoRevealer.StegoCore.StegoMethods;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;

namespace StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis
{
    /// <summary>
    /// Стегоанализатор метода Коха-Жао
    /// </summary>
    public class KzhaAnalyser
    {
        private const string MethodName = "Koch-Zhao method analysis";

        /// <summary>
        /// Параметры метода
        /// </summary>
        public KzhaParameters Params { get; set; }


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
            var result = new KzhaResult();
            result.Log($"Выполняется стегоанализ методом {MethodName} для файла изображения {Params.Image.ImgName}");

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
                catch { }

                result.ExtractedData = extractedData;  // Если включена попытка извлечения, будет string.Empty при неудаче
            }

            result.Log($"Стегоанализ методом {MethodName} завершён");
            return result;
        }

        // TODO: Перевести метод на параллельное выполнение (по набору коэффициентов)
        // TODO: После перевода на ImageBlocks и TraversalOptions сломался СА Коха-Жао
        /// <summary>
        /// Основная логика метода стегоанализа
        /// </summary>
        private KzhaResult InnerAnalyse(KzhaResult result)
        {
            var cSequences = new Dictionary<ScIndexPair, List<double>>();
            foreach (var coeff in Params.AnalysisCoeffs)
                cSequences.Add(coeff, new List<double>());

            var intervalStartIndexes = new Dictionary<ScIndexPair, int>();
            foreach (var coeff in Params.AnalysisCoeffs)
                intervalStartIndexes.Add(coeff, 0);

            // Разбиение на блоки и установка параметров обхода
            var traversalOptions = GetTraversalOptions();
            var blocks = new ImageBlocks(new ImageBlocksParameters(Params.Image, Params.GetBlockSize()));

            // Расчёт последовательности C
            var iterator = BlocksTraverseHelper.GetForLinearAccessBlock(blocks, traversalOptions);
            foreach (var block in iterator)
            {
                var dctBlock = FrequencyViewTools.DctBlock(block, Params.GetBlockSize());
                foreach (var coeff in Params.AnalysisCoeffs)
                    cSequences[coeff].Add(GetAbsDiff(dctBlock, coeff));
            }

            // Логирование cSequences, если оно включено
            if (Params.LoggingCSequences)
            {
                foreach (var coeff in Params.AnalysisCoeffs)
                {
                    string temp = $"\nПолная последовательность cSequence для набора коэффициентов ({coeff.FirstIndex}, {coeff.SecondIndex}):\n[";
                    foreach (var val in cSequences[coeff])
                        temp += string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:f2}, ", val);
                    temp = temp[..^2];
                    temp += "]\n";
                    result.Log(temp);
                }
            }

            // Поиск ступенчатого интервала
            foreach (var coeff in Params.AnalysisCoeffs)
            {
                // Получение непрерывного интервала аномально высоких значений cSequence
                (int indexLeft, int indexRight) = FindSuspiciousInterval(cSequences[coeff]);
                result.Log($"Для коэффициентов ({coeff.FirstIndex}, {coeff.SecondIndex}) получены следующие координаты интервала: [{indexLeft}:{indexRight}]");

                // Обрезка cSequences и сохранение оригинального индекса
                intervalStartIndexes[coeff] = indexLeft;  // Новый 0-й индекс в обрезанной cSequence на самом деле соответствует этому индексу последовательности
                cSequences[coeff] = cSequences[coeff].GetRange(indexLeft, indexRight - indexLeft + 1);

                var temp = $"Обрезанная последовательность cSequence для набора коэффициентов ({coeff.FirstIndex}, {coeff.SecondIndex}): ";
                foreach (var val in cSequences[coeff])
                    temp += string.Format("{0:f2} ", val);
                result.Log(temp);
            }

            // Расчёт предполагаемого порога скрытия
            var thresholds = new Dictionary<ScIndexPair, double>();  // Пороги подозрительных интервалов по наборам коэффициентов
            var indexes = new Dictionary<ScIndexPair, (int, int)?>();  // Индексы подозрительных интервалов по наборам коэффициентов

            foreach (var coeff in Params.AnalysisCoeffs)
            {
                bool detectedSecretData = cSequences[coeff].Count >= 8;  // Возможно ли извлечь хотя бы байт

                result.Log($"Для набора коэффициентов ({coeff.FirstIndex}, {coeff.SecondIndex}) " +
                    $"{(detectedSecretData ? "найден подозрительный интервал" : "не найден подозрительный интервал")}");

                // Запись подозрительного порога и интервала для текущего набора коэффициентов
                if (detectedSecretData)
                {
                    thresholds.Add(coeff, cSequences[coeff].Min());  // Порог - минимальное из значений cSequence
                    indexes.Add(coeff, (intervalStartIndexes[coeff], intervalStartIndexes[coeff] + cSequences[coeff].Count - 1));
                    result.Log($"Для набора коэффициентов ({coeff.FirstIndex}, {coeff.SecondIndex}) установлены значения: " +
                        $"Threshold (Порог) = {thresholds[coeff]}, Indexes (координаты ступенчатого всплеска) = ({indexes[coeff]?.Item1}, {indexes[coeff]?.Item2})");
                    result.SuspiciousIntervalIsFound = true;  // Считаем, что подозрительный интервал (хотя бы один) найден
                }
                else
                {
                    thresholds.Add(coeff, 0.0);
                    indexes.Add(coeff, (null));
                }
            }

            // Выбор наибольшего по порогу из подозрительных интервалов в качестве результирующего
            var maxVariant = thresholds.FirstOrDefault(val => val.Value == thresholds.Values.Max()).Key;  // Ключ - набор коэффициентов
            result.SuspiciousInterval = indexes[maxVariant];
            result.Threshold = thresholds[maxVariant];
            result.Coefficients = maxVariant;

            result.MessageBitsVolume = thresholds[maxVariant] > Params.Threshold && result.SuspiciousInterval is not null
                ? result.SuspiciousInterval.Value.rightInd - result.SuspiciousInterval.Value.leftInd + 1
                : 0;
            if (result.Threshold < Params.Threshold || result.MessageBitsVolume == 0)
                result.SuspiciousIntervalIsFound = false;  // Ранее факт обнаруженя мог быть установлен в true, но если результаты не удовлетворяют параметрам, то false

            result.Log($"В качестве результирующих выбраны коэффициенты ({maxVariant.FirstIndex}, {maxVariant.SecondIndex})");
            result.Log($"Факт наличия скрытой информации по итогу анализа {(result.SuspiciousIntervalIsFound ? "установлен" : "не установлен")}");

            return result;
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
        private double GetAbsDiff(double[,] block, ScIndexPair coeffs)
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
                throw new Exception("Length of cSequence must be grather than 1");

            var maxValue = cSequence.Max();  // Определяем максимальное из значений cSequence
            var thresholdValue = maxValue * Params.CutCoefficient;  // Определяем минимальный порог превышения, который будет учитывать как подозрительный  // TODO

            // Отсечение всех значений меньше порога (приведение их к нулю)
            var truncatedSequence = cSequence.Select(c => c < thresholdValue ? 0.0 : c).ToList();

            (int resultLeftIndex, int resultRightIndex) = (0, 0);  // Левый и правый индексы финальной подозрительноый последовательности

            // Формирование набора интервалов непрерывных высоких значений (превысивших порог) - получение их координат
            var intervalIndexes = new List<(int leftIndex, int rightIndex)>();
            int currentLeftIndex = 0;
            int currentRightIndex = 0;
            for (int i = 0; i < truncatedSequence.Count(); i++)
            {
                if (truncatedSequence[i] == 0.0)
                {
                    currentRightIndex = i - 1;
                    if (currentRightIndex - currentLeftIndex + 1 > 0)  // Если длина интервала при встрече "0" больше 0, то мы его записываем
                        intervalIndexes.Add((currentLeftIndex, currentRightIndex));
                    currentLeftIndex = i + 1;
                }
            }

            // Выбор наиболее длинного интервала
            if (intervalIndexes.Count() > 0)
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
        private ScIndexPair GetTwoMaximumsIndexes(List<double> sequence)
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
}
