using CsvHelper;
using CsvHelper.Configuration;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.Utils.Common.Entities;
using StegoRevealer.Utils.Common.Lib;
using StegoRevealer.Utils.CorrelationAnalyser.Entities;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace StegoRevealer.Utils.CorrelationAnalyser;

internal class Program
{
    private static CsvConfiguration CsvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = ",",
        HasHeaderRecord = true
    };

    private static double Alpha = 0.05;

    private static List<InputParameter> InputParameters = new List<InputParameter>()
    {
        new InputParameter
        {
            AvailableNames = new List<string> { "-alpha", "-a" },
            HasValue = true
        }
    };
    

    static async Task Main(string[] args)
    {
        if (!StartParametersHelper.IsParametersValid(args, InputParameters.ToArray()))
            return;
        var alphaParameterName = StartParametersHelper.GetSpecifiedParameter(args, InputParameters[0].AvailableNames);
        Alpha = StartParametersHelper.TryGetDoubleParameter(args, alphaParameterName ?? "") ?? 0.05;

        var startTime = DateTime.Now;
        ClearOutputDirectory();

        Console.WriteLine($"Запущен процесс анализа в {startTime}");

        var inputImages = Directory.GetFiles(Constants.InputDataImagesDirPath);
        inputImages = inputImages.Where(imgPath => Constants.ImagesExtensions.Contains(Path.GetExtension(imgPath))).ToArray();

        // Словарь всех результатов для всех анализируемых картинок
        var analysisResults = new ConcurrentBag<AnalysisResult>();

        // Создаём задачи анализа
        var imgAnalyseTasks = new List<Task>();
        foreach (var imgPath in inputImages)
            imgAnalyseTasks.Add(ImgAnalysisOperation(imgPath, analysisResults));

        // Ожидание анализа
        foreach (var imgTask in imgAnalyseTasks)
            await imgTask;

        var sortedAnalysisResults = analysisResults.OrderBy(r => r.Filename).ToList();

        // Вывод результатов
        Console.WriteLine();
        PrintAnalysisResult(sortedAnalysisResults);
        Console.WriteLine($"Завершён процесс анализа в {DateTime.Now} (начат в {startTime})");

        // Запись в файл
        WriteResultsToCsv(sortedAnalysisResults);

        // Подсчёт корреляций
        Console.WriteLine();
        CalcCorrelations(sortedAnalysisResults);
    }


    private static async Task ImgAnalysisOperation(string imgPath, ConcurrentBag<AnalysisResult> analysisResults)
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

        var analysisTasks = new List<Task>()
        {
            Task.Run(() => rsResult = rsAnalyzer.Analyse()),
            Task.Run(() => chiSqrResult = chiSqrAnalyzer.Analyse()),
            Task.Run(() => statmResult = statmAnalyzer.Analyse())
        };
        var renyiTestTask = RenyiEntropyTestCalc(img);

        foreach (var task in analysisTasks)
            await task;
        await renyiTestTask;

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
            NoiseValue = statmResult.NoiseValue,
            SharpnessValue = statmResult.SharpnessValue,
            BlurValue = statmResult.BlurValue,
            ContrastValue = statmResult.ContrastValue,
            EntropyShennonValue = statmResult.EntropyValues.Shennon,
            //EntropyVaidaValue = statmResult.EntropyValues.Vaida,
            //EntropyTsallisValue = statmResult.EntropyValues.Tsallis,
            EntropyRenyiValue = statmResult.EntropyValues.Renyi,
            //EntropyHavardValue = statmResult.EntropyValues.Havard,
            EntropyRenyiTestValues = renyiTestTask.Result
        };

        analysisResults.Add(newResultData);
        Console.WriteLine($"Завершён анализ для {filename}");
    }

    private static async Task<Dictionary<double, double>> RenyiEntropyTestCalc(ImageHandler img)
    {
        var renyiTestResults = new Dictionary<double, double>();
        int renyiTestAlphasNum = (int)((Constants.RenyiTestAlphaEndValue - Constants.RenyiTestAlphaStartValue + Constants.RenyiTestAlphaStep) / Constants.RenyiTestAlphaStep);

        var statmParams = new StatmParameters(img);
        var calculator = new EntropyCalculator(statmParams);

        var renyiTestTempResults = new ConcurrentDictionary<double, double>();
        var renyiTestTasks = new List<Task>();
        for (int i = 0; i < renyiTestAlphasNum; i++)
        {
            double renyiAlpha = Constants.RenyiTestAlphaStartValue + Constants.RenyiTestAlphaStep * i;
            renyiTestTasks.Add(Task.Run(() =>
            {
                double entropy = calculator.CalcRenyiEntropy(renyiAlpha);
                renyiTestTempResults.AddOrUpdate(renyiAlpha, entropy, (double key, double value) => entropy);
            }));
        }

        foreach (var task in renyiTestTasks)
            await task;

        foreach (var res in renyiTestTempResults)
            renyiTestResults.Add(res.Key, res.Value);

        return renyiTestResults;
    }

    private static void PrintAnalysisResult(List<AnalysisResult> analysisResults)
    {
        int maxImgNameLength = 0;
        foreach (var imgName in analysisResults.Select(r => r.Filename))
            if (imgName.Length > maxImgNameLength)
                maxImgNameLength = imgName.Length;

        Console.WriteLine("Результаты анализа:");
        foreach (var result in analysisResults)
        {
            var formattedValues = new List<string>()
            {
                FormattedValue("RS", result.RsValue),
                FormattedValue("ChiSqr", result.ChiSqrValue),
                FormattedValue("Noise", result.NoiseValue),
                FormattedValue("Sharpness", result.SharpnessValue),
                FormattedValue("Blur", result.BlurValue),
                FormattedValue("Contrast", result.ContrastValue),
                FormattedValue("Shennon", result.EntropyShennonValue),
                //FormattedValue("Vaida", result.EntropyVaidaValue),
                //FormattedValue("Tsallis", result.EntropyTsallisValue),
                FormattedValue("Renyi", result.EntropyRenyiValue),
                //FormattedValue("Havard", result.EntropyHavardValue)
            };

            Console.WriteLine($"\t" + string.Format("{0," + maxImgNameLength.ToString() + "}", result.Filename) + "." + string.Join(" ", formattedValues));
        }
    }
    private static string FormattedValue(string name, double value) => name + ": " + string.Format("{0,12:0000.0000000}", value) + ".";

    private static void WriteResultsToCsv(List<AnalysisResult> analysisResults)
    {
        using (var fileWriter = new StreamWriter(Constants.OutputAnalysisDataFilePath))
        using (var csvWriter = new CsvWriter(fileWriter, CsvConfig))
        {
            csvWriter.WriteRecords(analysisResults);
            csvWriter.Flush();
        }
    }

    private static string CalcRenyiTestCorrelations(List<AnalysisResult> analysisResults)
    {
        var rsArray = analysisResults.Select(r => r.RsValue).ToArray();
        var renyiTestArraysByAlpha = new Dictionary<double, double[]>();
        foreach (var renyiAlpha in analysisResults[0].EntropyRenyiTestValues.Keys)
            renyiTestArraysByAlpha.Add(renyiAlpha, analysisResults.Select(r => r.EntropyRenyiTestValues[renyiAlpha]).ToArray());

        var renyiTestCorrelations = new Dictionary<double, CorrelationValues>();
        foreach (var renyiAlpha in renyiTestArraysByAlpha.Keys)
        {
            var cv = CalcCorrelation(rsArray, renyiTestArraysByAlpha[renyiAlpha]);
            renyiTestCorrelations.Add(renyiAlpha, cv);
        }

        double maxRsToRenyiCorrelationValue = renyiTestCorrelations.Values.OrderByDescending(cv => cv.Spearman).First().Spearman;
        var renyiTestCorrelationsWithMaxCorrelation = renyiTestCorrelations.Where(rtc => rtc.Value.Spearman == maxRsToRenyiCorrelationValue);
        var renyiTestCorrelationsWithMaxCorrelationAlphas = renyiTestCorrelationsWithMaxCorrelation.Select(cv => cv.Key.Round(1));

        var renyiTestStringBuilder = new StringBuilder("Тест лучшей alpha корреляции Реньи:\n");
        foreach (var renyiTestCorrelation in renyiTestCorrelations.OrderBy(rtc => rtc.Key))
            renyiTestStringBuilder.Append($"\t\talpha = {renyiTestCorrelation.Key.Round(1):0.0}: {renyiTestCorrelation.Value.Spearman.Round(7)}\n");
        renyiTestStringBuilder.Append($"\tМаксимальная корреляция = {maxRsToRenyiCorrelationValue}\n\tВстречена с alpha: ");
        renyiTestStringBuilder.Append(string.Join(", ", renyiTestCorrelationsWithMaxCorrelationAlphas) + "\n");

        var maxCorrelationData = renyiTestCorrelationsWithMaxCorrelation.First();
        renyiTestStringBuilder.Append($"\tКорреляция при alpha корреляции Реньи = {maxCorrelationData.Key.Round(1)} " +
            $"{(maxCorrelationData.Value.PValue < maxCorrelationData.Value.Alpha ? "ЯВЛЯЕТСЯ" : "НЕ ЯВЛЯЕТСЯ")} значимой\n\n");

        string renyiTestStringResult = renyiTestStringBuilder.ToString();
        Console.WriteLine(renyiTestStringResult);
        return renyiTestStringResult;
    }

    private static void CalcCorrelations(List<AnalysisResult> analysisResults)
    {
        if (analysisResults.Count == 0)
            return;

        Console.WriteLine("Подсчёт корреляций:");

        var rsArray = analysisResults.Select(r => r.RsValue).ToArray();
        var chiSqrArray = analysisResults.Select(r => r.ChiSqrValue).ToArray();
        var noiseArray = analysisResults.Select(r => r.NoiseValue).ToArray();
        var sharpnessArray = analysisResults.Select(r => r.SharpnessValue).ToArray();
        var blurArray = analysisResults.Select(r => r.BlurValue).ToArray();
        var contrastArray = analysisResults.Select(r => r.ContrastValue).ToArray();
        var shennonArray = analysisResults.Select(r => r.EntropyShennonValue).ToArray();
        //var vaidaArray = analysisResults.Select(r => r.EntropyVaidaValue).ToArray();
        //var tsallisArray = analysisResults.Select(r => r.EntropyTsallisValue).ToArray();
        var renyiArray = analysisResults.Select(r => r.EntropyRenyiValue).ToArray();
        //var havardArray = analysisResults.Select(r => r.EntropyHavardValue).ToArray();

        var toWriteString = new StringBuilder();

        toWriteString.Append(EvaluateCorrelation("оценка по методу RS", "уровень шума", rsArray, noiseArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу RS", "уровень резкости", rsArray, sharpnessArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу RS", "уровень размытости", rsArray, blurArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу RS", "уровень контраста", rsArray, contrastArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу RS", "энтропия Шеннона", rsArray, shennonArray));
        //toWriteString.Append(EvaluateCorrelation("оценка по методу RS", "энтропия Вайда", rsArray, vaidaArray));
        //toWriteString.Append(EvaluateCorrelation("оценка по методу RS", "энтропия Цаллиса", rsArray, tsallisArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу RS", "энтропия Реньи", rsArray, renyiArray));
        //toWriteString.Append(EvaluateCorrelation("оценка по методу RS", "энтропия Хаварда", rsArray, havardArray));

        toWriteString.Append(EvaluateCorrelation("оценка по методу Хи-квадрат", "уровень шума", chiSqrArray, noiseArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу Хи-квадрат", "уровень резкости", chiSqrArray, sharpnessArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу Хи-квадрат", "уровень размытости", chiSqrArray, blurArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу Хи-квадрат", "уровень контраста", chiSqrArray, contrastArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу Хи-квадрат", "энтропия Шеннона", chiSqrArray, shennonArray));
        //toWriteString.Append(EvaluateCorrelation("оценка по методу Хи-квадрат", "энтропия Вайда", chiSqrArray, vaidaArray));
        //toWriteString.Append(EvaluateCorrelation("оценка по методу Хи-квадрат", "энтропия Цаллиса", chiSqrArray, tsallisArray));
        toWriteString.Append(EvaluateCorrelation("оценка по методу Хи-квадрат", "энтропия Реньи", chiSqrArray, renyiArray));
        //toWriteString.Append(EvaluateCorrelation("оценка по методу Хи-квадрат", "энтропия Хаварда", chiSqrArray, havardArray));

        toWriteString.Append(EvaluateCorrelation("уровень резкости", "уровень размытости", sharpnessArray, blurArray));
        //toWriteString.Append(EvaluateCorrelation("энтропия Вайда", "энтропия Цаллиса", vaidaArray, tsallisArray));
        //toWriteString.Append(EvaluateCorrelation("энтропия Цаллиса", "энтропия Реньи", tsallisArray, renyiArray));
        //toWriteString.Append(EvaluateCorrelation("энтропия Реньи", "энтропия Хаварда", renyiArray, havardArray));

        toWriteString.Append(CalcRenyiTestCorrelations(analysisResults));

        File.WriteAllText(Constants.OutputCorrelationFilePath, toWriteString.ToString());
    }

    private static string EvaluateCorrelation(string dataAName, string dataBName, IEnumerable<double> dataA, IEnumerable<double> dataB)
    {
        if (dataA.Count() != dataB.Count())
            throw new Exception("Sizes of arrays must be equal");

        var cv = CalcCorrelation(dataA, dataB);
        var str = new StringBuilder();

        str.Append($"Корреляция между значениями: {dataAName} и {dataBName} (заданный уровень значимости: {cv.Alpha})");
        str.Append($"\tЗначение корреляции Спирмена: {cv.Spearman}\n");
        str.Append($"\tЗначение t-статистики Стьюдента: {(cv.Spearman == 1 ? "inf" : cv.TValue)} (критическое t-значение: {cv.CriticalTValue})\n");
        str.Append($"\tp-значение: {cv.PValue}\n");
        str.Append($"\tКорреляция между значениями '{dataAName}' и '{dataBName}' {(cv.PValue < cv.Alpha ? "ЯВЛЯЕТСЯ" : "НЕ ЯВЛЯЕТСЯ")} значимой\n\n");

        Console.Write(str);
        return str.ToString();
    }

    private static CorrelationValues CalcCorrelation(IEnumerable<double> dataA, IEnumerable<double> dataB)
    {
        /*
         * Величина коэффициента корреляции Спирмена лежит в интервале +1 и -1. Он, как и коэффициент Пирсона, может быть положительным и отрицательным,
         * характеризуя направленность связи между двумя признаками, измеренными в ранговой шкале.
         * Если нет оснований отвергнуть нулевую гипотезу, ранговая корреляционная связь не значима. В противном случае существует значимая ранговая корреляционная связь.
         * Нулевая гипотеза = "коэффициент корреляции незначим", а а dataA и dataB некоррелированны (не связаны линейной зависимостью).
         */

        int n = dataA.Count();  // Размер выборки
        double alpha = Alpha;  // Уровень значимости - это пороговое значение p-значения, при котором нулевая гипотеза отвергается.
                               // Если вероятность (p-значение) наблюдаемого результата меньше или равна уровню значимости, нулевая гипотеза отвергается.
                               // По сути: если вероятность нулевой гипотезы (корреляции нет) при наблюдаемых выборках меньше alpha, то гипотеза неверна (корреляция есть).

        var spearman = CalcSpearman(dataA, dataB);  // Коэффициент корреляции Спирмена
        var tValue = CalcTValue(spearman, n);  // t-значение Стьюдента (t-статистика Стьюдента).
                                               // Это мера, используемая в t-тесте для оценки различий между средними значениями двух выборок.
        var criticalTValue = GetCriticalTValue(alpha, n);  // Критическое значение t-критерия Стьюдента
        var pValue = CalcPValue(tValue, n);  // p-значение - вероятность получить наблюдаемые результаты при верности нулевой гипотезы
                                             // (т.е. веронятность получить наблюдаемое значение при нулевой гипотезе - "dataA и dataB некоррелированны").
                                             // P-значение говорит от значимости как таковой, но не характеризует "величину эффекта".

        return new CorrelationValues
        {
            N = n,
            Alpha = alpha,
            Spearman = spearman,
            TValue = tValue,
            CriticalTValue = criticalTValue,
            PValue = pValue
        };
    }

    private static double CalcSpearman(IEnumerable<double> dataA, IEnumerable<double> dataB) => Correlation.Spearman(dataA, dataB);
    private static double CalcTValue(double spearman, int n) => spearman * Math.Sqrt((n - 2) / (1 - Math.Pow(spearman, 2)));
    private static double CalcPValue(double tValue, double n) => 2 * StudentT.CDF(location: 0.0, scale: 1.0, freedom: n - 2, x: -Math.Abs(tValue));
    private static double GetCriticalTValue(double alpha, double n) => StudentT.InvCDF(location: 0.0, scale: 1.0, freedom: n - 2, p: 1.0 - alpha / 2.0);

    /*
     * T-тест — это статистический тест, используемый для сравнения средних значений двух групп или выборок, чтобы определить, 
     * являются ли различия между ними статистически значимыми.
     * Для расчета t-теста используется t-статистика (= t-значение), которая показывает, насколько далеко среднее выборки находится от среднего значения популяции 
     * (или другой выборки) в единицах стандартной ошибки. На основании полученного t-значения и степеней свободы (зависит от размеров выборок) вычисляется p-значение.
     */


    private static void ClearOutputDirectory()
    {
        try
        {
            Directory.Delete(Constants.OutputDirPath, true);
        }
        catch { }

        if (!Directory.Exists(Constants.InputDataImagesDirPath))
            Directory.CreateDirectory(Constants.InputDataImagesDirPath);

        if (!Directory.Exists(Constants.OutputDirPath))
            Directory.CreateDirectory(Constants.OutputDirPath);
    }
}
