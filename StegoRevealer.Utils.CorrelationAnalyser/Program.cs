using CsvHelper;
using CsvHelper.Configuration;
using MathNet.Numerics.Distributions;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.Utils.CorrelationAnalyser.Entities;
using StegoRevealer.Utils.DataPreparer.Lib.TaskPool;
using System.Collections.Concurrent;
using System.Globalization;

namespace StegoRevealer.Utils.CorrelationAnalyser;

internal class Program
{
    private static CsvConfiguration CsvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = ",",
        HasHeaderRecord = true
    };

    private static double GetTableStudent(double p, double stepsOfFreedom)
    {
        // return StudentT.InvCDF(location: 0.0, scale: 1.0, freedom: stepsOfFreedom, p: 1.0 - p / 2.0);
        // same as
        return Math.Abs(StudentT.InvCDF(location: 0.0, scale: 1.0, freedom: stepsOfFreedom, p: p / 2.0));
    }

    static void Main(string[] args)
    {
        var startTime = DateTime.Now;
        Console.WriteLine($"Запущен процесс анализа в {startTime}");

        var inputImages = Directory.GetFiles(Constants.InputDataImagesDirPath);
        inputImages = inputImages.Where(imgPath => Constants.ImagesExtensions.Contains(Path.GetExtension(imgPath))).ToArray();

        // Словарь всех результатов для всех анализируемых картинок
        var analysisResults = new ConcurrentBag<AnalysisResult>();

        // Создаём задачи анализа
        var taskPool = new TaskPool(considerOnlyRunningTasks: false);

        var imgAnalyseTasks = new List<Task>();
        foreach (var imgPath in inputImages)
        {
            imgAnalyseTasks.Add(taskPool.AddAsync(() => Task.Run(() =>
            {
                string filename = Path.GetFileNameWithoutExtension(imgPath);
                Console.WriteLine($"Начат анализ для {filename}");

                var img = new ImageHandler(imgPath.ToString());
                var rsAnalyzer = new RsAnalyser(img);
                var chiSqrAnalyzer = new ChiSquareAnalyser(img);
                var statmAnalyzer = new StatmAnalyser(img);

                RsResult? rsResult = null;
                ChiSquareResult? chiSqrResult = null;
                StatmResult? statmResult = null;

                var rsTask = Task.Run(() => rsResult = rsAnalyzer.Analyse());
                var chiSqrTask = Task.Run(() => chiSqrResult = chiSqrAnalyzer.Analyse());
                var statmTask = Task.Run(() => statmResult = statmAnalyzer.Analyse());

                rsTask.Wait();
                chiSqrTask.Wait();
                statmTask.Wait();

                if (rsResult is null || chiSqrResult is null || statmResult is null)
                {
                    Console.WriteLine($"Ошибка получения результата для {filename}");
                    return;
                }

                var newResultData = new AnalysisResult
                {
                    Filename = filename,
                    ChiSqrValue = chiSqrResult.MessageRelativeVolume,
                    RsValue = rsResult.MessageRelativeVolume,
                    NoiseValue = statmResult.NoiseValueMethod2,
                    SharpnessValue = statmResult.SharpnessValue,
                    BlurValue = statmResult.BlurValue,
                    ContrastValue = statmResult.ContrastValue,
                    EntropyShennonValue = statmResult.EntropyValues.Shennon,
                    EntropyVaidaValue = statmResult.EntropyValues.Vaida,
                    EntropyTsallisValue = statmResult.EntropyValues.Tsallis,
                    EntropyRenyiValue = statmResult.EntropyValues.Renyi,
                    EntropyHavardValue = statmResult.EntropyValues.Havard
                };

                analysisResults.Add(newResultData);
                Console.WriteLine($"Завершён анализ для {filename}");
            })));
        }

        // Ожидание анализа
        foreach (var imgTask in imgAnalyseTasks)
            imgTask.Wait();

        var sortedAnalysisResults = analysisResults.OrderBy(r => r.Filename).ToList();

        // Вывод результатов
        int maxImgNameLength = 0;
        foreach (var imgName in sortedAnalysisResults.Select(r => r.Filename))
            if (imgName.Length > maxImgNameLength)
                maxImgNameLength = imgName.Length;

        Console.WriteLine();
        Console.WriteLine("Результаты анализа:");
        foreach (var result in sortedAnalysisResults)
        {
            Console.WriteLine($"\t"
                + string.Format("{0," + maxImgNameLength.ToString() + "}", result.Filename) + ". " +
                "RS: " + string.Format("{0,7:000.00%}", result.RsValue) + ". " +
                "ChiSqr: " + string.Format("{0,7:000.00%}", result.ChiSqrValue) + ". " +
                "Noise: " + string.Format("{0,11:000.0000000}", result.NoiseValue) + ". " +
                "Sharpness: " + string.Format("{0,12:000.0000000}", result.SharpnessValue) + ". " +
                "Blur: " + string.Format("{0,11:000.0000000}", result.BlurValue) + ". " +
                "Contrast: " + string.Format("{0,11:000.0000000}", result.ContrastValue) + ". " +
                "Shennon: " + string.Format("{0,11:000.0000000}", result.EntropyShennonValue) + ". " +
                "Vaida: " + string.Format("{0,11:000.0000000}", result.EntropyVaidaValue) + ". " +
                "Tsallis: " + string.Format("{0,11:000.0000000}", result.EntropyTsallisValue) + ". " +
                "Renyi: " + string.Format("{0,11:000.0000000}", result.EntropyRenyiValue) + ". " +
                "Havard: " + string.Format("{0,11:0000.0000000}", result.EntropyHavardValue) + ".");
        }

        Console.WriteLine($"Завершён процесс анализа в {DateTime.Now} (начат в {startTime})");

        // Запись в файл
        using (var fileWriter = new StreamWriter(Constants.OutputAnalysisDataFilePath))
        using (var csvWriter = new CsvWriter(fileWriter, CsvConfig))
        {
            csvWriter.WriteRecords(analysisResults);
            csvWriter.Flush();
        }

        // Подсчёт корреляций
        var rsArray = sortedAnalysisResults.Select(r => r.RsValue).ToArray();
        var chiSqrArray = sortedAnalysisResults.Select(r => r.ChiSqrValue).ToArray();
        var noiseArray = sortedAnalysisResults.Select(r => r.NoiseValue).ToArray();
        var sharpnessArray = sortedAnalysisResults.Select(r => r.SharpnessValue).ToArray();
        var blurArray = sortedAnalysisResults.Select(r => r.BlurValue).ToArray();
        var contrastArray = sortedAnalysisResults.Select(r => r.ContrastValue).ToArray();
        var shennonArray = sortedAnalysisResults.Select(r => r.EntropyShennonValue).ToArray();
        var vaidaArray = sortedAnalysisResults.Select(r => r.EntropyVaidaValue).ToArray();
        var tsallisArray = sortedAnalysisResults.Select(r => r.EntropyTsallisValue).ToArray();
        var renyiArray = sortedAnalysisResults.Select(r => r.EntropyRenyiValue).ToArray();
        var havardArray = sortedAnalysisResults.Select(r => r.EntropyHavardValue).ToArray();

        int n = sortedAnalysisResults.Count;
        var tValue = GetTableStudent(0.001, n - 2);  // Число степеней свободы = n - 2

        Console.WriteLine($"Табличное значение критерия Стьюдента при степенях свободы {n - 2}: {tValue}\n");

        double rsToNoiseSpearman = MathNet.Numerics.Statistics.Correlation.Spearman(rsArray, noiseArray);
        double tValueRsToNoiseSpearman = CalcTValue(rsToNoiseSpearman, n);
        PrintCorrelationResult(rsToNoiseSpearman, tValueRsToNoiseSpearman, tValueRsToNoiseSpearman > tValue, "оценка по методу RS", "уровень шума");

        double chiToNoiseSpearman = MathNet.Numerics.Statistics.Correlation.Spearman(chiSqrArray, noiseArray);
        double tValueChiToNoiseSpearman = CalcTValue(chiToNoiseSpearman, n);
        PrintCorrelationResult(chiToNoiseSpearman, tValueChiToNoiseSpearman, tValueChiToNoiseSpearman > tValue, "оценка по методу ChiSqr", "уровень шума");
    }

    private static void PrintCorrelationResult(double spearman, double tValue, bool isValuable, string firstValueName, string secondValueName)
    {
        Console.WriteLine($"Корреляция между значениями: {firstValueName} и {secondValueName}");
        Console.WriteLine($"\tЗначение корреляции Спирмена: {spearman}");
        Console.WriteLine($"\tЗначение t-критерия: {tValue}");
        Console.WriteLine($"\tКорреляция между значениями '{firstValueName}' и '{secondValueName}' {(isValuable ? "ЯВЛЯЕТСЯ" : "НЕ ЯВЛЯЕТСЯ")} значимой");
        Console.WriteLine();
    }

    private static double CalcTValue(double spearmanValue, int n) => spearmanValue * Math.Sqrt((n - 2) / (1 - Math.Pow(spearmanValue, 2)));
}
