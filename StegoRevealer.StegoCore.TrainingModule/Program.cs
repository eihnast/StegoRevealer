using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.ML;
using StegoRevealer_StegoCore_TrainingModule;
using System.Globalization;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.AnalysisMethods.HcfCom;
using Newtonsoft.Json;

namespace StegoRevealer.StegoCore.TrainingModule;

internal static class Program
{
    static void Main(string[] args)
    {
        string originPath = "e:\\Temp\\Steganalysis\\OriginImages\\";
        string targetPath = "e:\\Temp\\Steganalysis\\Обучение метода FAN\\ClearImages\\";

        var filenames = Directory.GetFiles(originPath);
        var rnd = new Random();
        rnd.Shuffle(filenames);

        var resultFilenames = new List<string>();
        foreach (var file in filenames[0..100])
        {
            string fileName = Path.GetFileName(file);
            string newFileName = Path.Combine(targetPath, fileName);
            File.Copy(file, newFileName, true);
            resultFilenames.Add(newFileName);
        }

        var comLists = new List<double[]>();
        foreach (var file in resultFilenames)
        {
            var img = new ImageHandler(file);
            var comList = FanAnalyser.ComputeCompositeCom(img);
            Console.WriteLine($"Calculated for {file}");
            comLists.Add(comList);
        }

        var serializedComLists = JsonConvert.SerializeObject(comLists);
        Console.WriteLine(serializedComLists);

        //Console.WriteLine("Проверка моделей на датаестах обучения, для проверки взяты 20% последних данных");
        //CheckMlModels();

        //Console.WriteLine("Проверка моделей на специальном тестовом датасете");
        //TestMlModels();

        //Console.WriteLine("Ручной подсчёт значений матрицы ошибок на тестовом датасете");
        //CalcValuesForTestDataSet("TestData\\MlAnalysisDataTesting_ForComplexSa.csv");

        //Console.WriteLine("Проверка модели на тестовом датасете в 2 НЗБ");
        //TestMlModel2Lsb();
        //CalcValuesForTestDataSet("TestData\\MlAnalysisDataTesting2Lsb_ForComplexSa.csv");
    }

    private static void TestMlModel2Lsb()
    {
        TestModel<DecisionModel_ComplexSa.ModelInput>(
            "TestData\\MlAnalysisDataTesting2Lsb_ForComplexSa.csv", "DecisionModel_ComplexSa", firstTwenty: null);
    }

    private static void CalcValuesForTestDataSet(string dataPath)
    {
        // Загружаем CSV и обрабатываем строки
        var processedData = File.ReadLines(dataPath)
            .Select(line => line.Replace(',', '.')) // Меняем запятые на точки
            .ToList();

        // Записываем исправленный файл
        var processedDataFileName =
            Path.Combine(Path.GetDirectoryName(dataPath) ?? "", Path.GetFileNameWithoutExtension(dataPath) + "_DotDecimalSeparator" + Path.GetExtension(dataPath));
        File.WriteAllLines(processedDataFileName, processedData);

        var testData = LoadData<DecisionModel_ComplexSa.ModelInput>(processedDataFileName);
        var results = new Dictionary<DecisionModel_ComplexSa.ModelInput, bool>();
        var resultsProbabilities = new Dictionary<DecisionModel_ComplexSa.ModelInput, double>();

        foreach (var imgData in testData)
        {
            var clearImgData = new DecisionModel_ComplexSa.ModelInput
            {
                ChiSqrHorizontalRelativeVolume = imgData.ChiSqrHorizontalRelativeVolume,
                ChiSqrVerticalRelativeVolume = imgData.ChiSqrVerticalRelativeVolume,
                RsRelativeVolume = imgData.RsRelativeVolume,
                KzhaHorizontalBitsVolume = imgData.KzhaHorizontalBitsVolume,
                KzhaHorizontalThreshold = imgData.KzhaHorizontalThreshold,
                KzhaVerticalBitsVolume = imgData.KzhaVerticalThreshold,
                KzhaVerticalThreshold = imgData.KzhaVerticalThreshold,
                Noise = imgData.Noise,
                Sharpness = imgData.Sharpness,
                Blur = imgData.Blur,
                Contrast = imgData.Contrast,
                EntropyShennon = imgData.EntropyShennon,
                EntropyRenyi11 = imgData.EntropyRenyi11,
                PixelsNum = imgData.PixelsNum
            };
            var predictResult = DecisionModel_ComplexSa.Predict(clearImgData);

            results.Add(imgData, predictResult.PredictedLabel);  // IsDataHided ?
            resultsProbabilities.Add(imgData, predictResult.Probability);
        }

        int truePositive = results.Count(r => r.Key.IsDataHided && r.Value);
        int trueNegative = results.Count(r => !r.Key.IsDataHided && !r.Value);
        int falsePositive = results.Count(r => !r.Key.IsDataHided && r.Value);
        int falseNegative = results.Count(r => r.Key.IsDataHided && !r.Value);

        Console.WriteLine("Матрциа ошибок:");
        Console.WriteLine($"\tПравильноположительных: TP (True Positive) = {truePositive}");
        Console.WriteLine($"\tПравильноотрицательных: TN (True Negative) = {trueNegative}");
        Console.WriteLine($"\tЛожноположительных: FP (False Positive) = {falsePositive}");
        Console.WriteLine($"\tЛожноотрицательных: FN (False Negative) = {falseNegative}");

        double accuracy = CalcAccuracy(truePositive, trueNegative, falsePositive, falseNegative);
        double precision = CalcPrecision(truePositive, falsePositive);
        double recall = CalcRecall(truePositive, falseNegative);
        double f1Score = CalcF1Score(precision, recall);

        Console.WriteLine("Значения метрик:");
        Console.WriteLine($"\tAccuracy: {accuracy}");  // точность классификации.
        Console.WriteLine($"\tPrecision: {precision}");  // точность предсказания положительных примеров.
        Console.WriteLine($"\tRecall: {recall}");  // полнота предсказания положительных примеров.
        Console.WriteLine($"\tF1 Score: {f1Score}");  // гармоническое среднее между Precision и Recall.
        Console.WriteLine();

        var dataForAucs = testData.Select(td => new { realNum = td.IsDataHided ? 1 : 0, probability = resultsProbabilities[td] });
        Console.WriteLine("Вывод данных:");
        Console.WriteLine("realNum:");
        Console.WriteLine(string.Join("; ", dataForAucs.Select(dfa => dfa.realNum.ToString())));
        Console.WriteLine("probability:");
        Console.WriteLine(string.Join("; ", dataForAucs.Select(dfa => dfa.probability.ToString())));
        Console.WriteLine();
    }

    private static double CalcAccuracy(int tp, int tn, int fp, int fn)
        => (double)(tp + tn) / (double)(tp + tn + fp + fn);
    private static double CalcPrecision(int tp, int fp)
        => (double)tp / (double)(tp + fp);
    private static double CalcRecall(int tp, int fn)
        => (double)tp / (double)(tp + fn);
    private static double CalcF1Score(double precision, double recall)
        => 2 * ((precision * recall) / (precision + recall));

    private static List<T> LoadData<T>(string path)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
            Delimiter = ";"
        };

        List<T> records = new List<T>();
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, csvConfig))
        {
            var rawRecords = csv.GetRecords<T>();
            records = rawRecords.ToList();
        }

        return records;
    }

    private static void TestMlModels()
    {
        bool? firstTwenty = null;

        TestModel<DecisionModel_ComplexSa.ModelInput>(
            "TestData\\MlAnalysisDataTesting_ForComplexSa.csv", "DecisionModel_ComplexSa", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaNoQuality.ModelInput>(
            "TestData\\MlAnalysisDataTesting_ForComplexSaNoQuality.csv", "DecisionModel_ComplexSaNoQuality", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaNoVertical.ModelInput>(
            "TestData\\MlAnalysisDataTesting_ForComplexSaNoVertical.csv", "DecisionModel_ComplexSaNoVertical", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaNoPixelsNum.ModelInput>(
            "TestData\\MlAnalysisDataTesting_ForComplexSaNoPixelsNum.csv", "DecisionModel_ComplexSaNoPixelsNum", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaNoKzha.ModelInput>(
            "TestData\\MlAnalysisDataTesting_ForComplexSaNoKzha.csv", "DecisionModel_ComplexSaNoKzha", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaOnlySaMethods.ModelInput>(
            "TestData\\MlAnalysisDataTesting_ForComplexSaOnlySaMethods.csv", "DecisionModel_ComplexSaOnlySaMethods", firstTwenty: firstTwenty);
    }

    private static void CheckMlModels()
    {
        bool firstTwenty = false;

        TestModel<DecisionModel_ComplexSa.ModelInput>(
            "TrainingData\\MlAnalysisData_ComplexSa.csv", "DecisionModel_ComplexSa", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaNoQuality.ModelInput>(
            "TrainingData\\MlAnalysisData_ComplexSaNoQuality.csv", "DecisionModel_ComplexSaNoQuality", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaNoVertical.ModelInput>(
            "TrainingData\\MlAnalysisData_ComplexSaNoVertical.csv", "DecisionModel_ComplexSaNoVertical", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaNoPixelsNum.ModelInput>(
            "TrainingData\\MlAnalysisData_ComplexSaNoPixelsNum.csv", "DecisionModel_ComplexSaNoPixelsNum", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaNoKzha.ModelInput>(
            "TrainingData\\MlAnalysisData_ComplexSaNoKzha.csv", "DecisionModel_ComplexSaNoKzha", firstTwenty: firstTwenty);
        TestModel<DecisionModel_ComplexSaOnlySaMethods.ModelInput>(
            "TrainingData\\MlAnalysisData_ComplexSaOnlySaMethods.csv", "DecisionModel_ComplexSaOnlySaMethods", firstTwenty: firstTwenty);
    }

    private static void TestModel<T>(string dataPath, string modelName, bool? firstTwenty = false)
    {
        // Загружаем CSV и обрабатываем строки
        var processedData = File.ReadLines(dataPath)
            .Select(line => line.Replace(',', '.')) // Меняем запятые на точки
            .ToList();

        // Удаляем первые 80% или берём первые 20%
        int len = processedData.Count;
        int trainingDataLen = len / 5;
        int learningDataLen = len - trainingDataLen;

        if (firstTwenty is not null && firstTwenty.Value)
            processedData = processedData.Take(trainingDataLen).ToList();
        else if (firstTwenty is not null && !firstTwenty.Value)
            processedData = processedData.Skip(learningDataLen).ToList();

        // Записываем исправленный файл
        var processedDataFileName = 
            Path.Combine(Path.GetDirectoryName(dataPath) ?? "", Path.GetFileNameWithoutExtension(dataPath) + "_DotDecimalSeparator" + Path.GetExtension(dataPath));
        File.WriteAllLines(processedDataFileName, processedData);

        // Создаем ML-контекст
        MLContext mlContext = new MLContext();

        // Загружаем модель
        ITransformer trainedModel = mlContext.Model.Load($"{modelName}.mlnet", out var _);

        IDataView testData = mlContext.Data.LoadFromTextFile<T>(
            path: processedDataFileName, separatorChar: ';', hasHeader: true);

        var predictions = trainedModel.Transform(testData);
        var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: "IsDataHided");

        // Выводим метрики
        Console.WriteLine(modelName);
        Console.WriteLine($"\tAccuracy: {metrics.Accuracy}");  // точность классификации.
        Console.WriteLine($"\tPrecision: {metrics.PositivePrecision}");  // точность предсказания положительных примеров.
        Console.WriteLine($"\tRecall: {metrics.PositiveRecall}");  // полнота предсказания положительных примеров.
        Console.WriteLine($"\tF1 Score: {metrics.F1Score}");  // гармоническое среднее между Precision и Recall.
        Console.WriteLine($"\tAUC: {metrics.AreaUnderRocCurve}");  // площадь под ROC-кривой.
        Console.WriteLine($"\tAUCPR: {metrics.AreaUnderPrecisionRecallCurve}");  // площадь под кривой Precision-Recall.
        Console.WriteLine();
    }
}
