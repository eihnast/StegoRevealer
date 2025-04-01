using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.DecisionModule;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Diagnostics;

namespace StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;

public static class JointAnalysisStarter
{
    public async static Task<JointAnalysisResults> Start(JointAnalysisParameters parameters)
    {
        var results = CoreHelper.CreateValuesByAnalysisMethodDictionary<ILoggedAnalysisResult>();  // Словарь результатов
        var methodTasks = CoreHelper.CreateValuesByAnalysisMethodDictionary<Task<ILoggedAnalysisResult>>();  // Словарь задач

        var timer = Stopwatch.StartNew();  // Запуск таймера - подсчёт времени работы непосредственно методов стегоанализа

        // Создание задач
        if (parameters.ChiSquareParameters is not null)  // Хи-квадрат
        {
            var chiSqrMethodAnalyzer = new ChiSquareAnalyser(parameters.ChiSquareParameters);
            methodTasks[AnalysisMethod.ChiSquare] = Task.Run(() => chiSqrMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.RsParameters is not null)  // RS
        {
            var rsMethodAnalyzer = new RsAnalyser(parameters.RsParameters);
            methodTasks[AnalysisMethod.RegularSingular] = Task.Run(() => rsMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.KzhaParameters is not null)  // KZHA
        {
            var kzhaMethodAnalyzer = new KzhaAnalyser(parameters.KzhaParameters);
            methodTasks[AnalysisMethod.KochZhaoAnalysis] = Task.Run(() => kzhaMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.Image is not null)
        {
            var statmAnalyzer = new StatmAnalyser(parameters.Image);
            methodTasks[AnalysisMethod.Statm] = Task.Run(() => statmAnalyzer.Analyse() as ILoggedAnalysisResult);
        }

        // Запуск комплексного стегоанализа
        var complexMethodTasks = new List<Task>();
        ComplexSaMethodResults? complexSaMethodResults = null;
        if (parameters.Image is not null && parameters.UseComplexMethod is true)
        {
            complexSaMethodResults = new ComplexSaMethodResults();

            var complexChiSqrMethodHorizontalAnalyzer = new ChiSquareAnalyser(parameters.Image);
            complexChiSqrMethodHorizontalAnalyzer.Params.TraverseType = TraverseType.Horizontal;

            var complexChiSqrMethodVerticalAnalyzer = new ChiSquareAnalyser(parameters.Image);
            complexChiSqrMethodVerticalAnalyzer.Params.TraverseType = TraverseType.Vertical;

            var complexRsMethodAnalyzer = new RsAnalyser(parameters.Image);

            var complexKzhaMethodHorizontalAnalyzer = new KzhaAnalyser(parameters.Image);
            complexKzhaMethodHorizontalAnalyzer.Params.TraverseType = TraverseType.Horizontal;

            var complexKzhaMethodVerticalAnalyzer = new KzhaAnalyser(parameters.Image);
            complexKzhaMethodVerticalAnalyzer.Params.TraverseType = TraverseType.Horizontal;

            var statmAnalyzer = new StatmAnalyser(parameters.Image);
            statmAnalyzer.Params.EntropyCalcSensitivity = 1.1;

            complexSaMethodResults.PixelsNum = parameters.Image.Width * parameters.Image.Height;

            // Задачи
            complexMethodTasks.Add(Task.Run(() => { 
                try { complexSaMethodResults.ChiSquareHorizontalResult = complexChiSqrMethodHorizontalAnalyzer.Analyse(); } 
                catch { /* Не обрабатываем, т.к. позже проверим результат на null */ } }));
            complexMethodTasks.Add(Task.Run(() => { 
                try { complexSaMethodResults.ChiSquareVerticalResult = complexChiSqrMethodVerticalAnalyzer.Analyse(); } 
                catch { /* Не обрабатываем, т.к. позже проверим результат на null */ } }));
            complexMethodTasks.Add(Task.Run(() => { 
                try { complexSaMethodResults.RsResult = complexRsMethodAnalyzer.Analyse(); } 
                catch { /* Не обрабатываем, т.к. позже проверим результат на null */ } }));
            complexMethodTasks.Add(Task.Run(() => { 
                try { complexSaMethodResults.KzhaHorizontalResult = complexKzhaMethodHorizontalAnalyzer.Analyse(); } 
                catch { /* Не обрабатываем, т.к. позже проверим результат на null */ } }));
            complexMethodTasks.Add(Task.Run(() => { 
                try { complexSaMethodResults.KzhaVerticalResult = complexKzhaMethodVerticalAnalyzer.Analyse(); } 
                catch { /* Не обрабатываем, т.к. позже проверим результат на null */ } }));
            complexMethodTasks.Add(Task.Run(() => { 
                try { complexSaMethodResults.StatmResult = statmAnalyzer.Analyse(); } 
                catch { /* Не обрабатываем, т.к. позже проверим результат на null */ } }));
        }

        // Ожидание
        foreach (var methodTask in methodTasks)
            if (methodTask.Value is not null)
                await methodTask.Value;
        foreach (var complesMethodTask in complexMethodTasks)
            await complesMethodTask;

        // Сбор результатов
        foreach (AnalysisMethod method in Enum.GetValues(typeof(AnalysisMethod)))
            results[method] = methodTasks[method]?.Result;

        timer.Stop();  // Остановка таймера

        return ProcessResults(parameters.Image, results, timer, complexSaMethodResults);
    }

    private static JointAnalysisResults ProcessResults(
        ImageHandler? image, Dictionary<AnalysisMethod, ILoggedAnalysisResult?> results, Stopwatch timer, 
        ComplexSaMethodResults? complexSaMethodResults = null)  // bool decisionCanBeCalculated
    {
        // Приведение к известным типам результатов
        var chiRes = results[AnalysisMethod.ChiSquare] as ChiSquareResult;
        var rsRes = results[AnalysisMethod.RegularSingular] as RsResult;
        var kzhaRes = results[AnalysisMethod.KochZhaoAnalysis] as KzhaResult;
        var statmRes = results[AnalysisMethod.Statm] as StatmResult;

        // Формирование вывода
        bool? isHidingDetected = null;
        if (complexSaMethodResults is not null)
        {
            var saResult = new SteganalysisResults
            {
                ChiSquareHorizontalVolume = complexSaMethodResults.ChiSquareHorizontalResult.MessageRelativeVolume,
                ChiSquareVerticalVolume = complexSaMethodResults.ChiSquareVerticalResult.MessageRelativeVolume,
                RsVolume = complexSaMethodResults.RsResult.MessageRelativeVolume,
                KzhaHorizontalThreshold = complexSaMethodResults.KzhaHorizontalResult.Threshold,
                KzhaHorizontalMessageBitVolume = complexSaMethodResults.KzhaHorizontalResult.MessageBitsVolume,
                KzhaVerticalThreshold = complexSaMethodResults.KzhaVerticalResult.Threshold,
                KzhaVerticalMessageBitVolume = complexSaMethodResults.KzhaVerticalResult.MessageBitsVolume,
                NoiseValue = complexSaMethodResults.StatmResult.NoiseValue,
                SharpnessValue = complexSaMethodResults.StatmResult.SharpnessValue,
                BlurValue = complexSaMethodResults.StatmResult.BlurValue,
                ContrastValue = complexSaMethodResults.StatmResult.ContrastValue,
                EntropyShennonValue = complexSaMethodResults.StatmResult.EntropyValues.Shennon,
                EntropyRenyiValue = complexSaMethodResults.StatmResult.EntropyValues.Renyi,
                PixelsNumber = (image?.Width ?? 0) * (image?.Height ?? 0)
            };

            isHidingDetected = SteganalysisDecision.Calculate(saResult);
        }

        return new JointAnalysisResults
        {
            ChiSquareResult = chiRes,
            RsResult = rsRes,
            KzhaResult = kzhaRes,
            StatmResult = statmRes,
            ElapsedTime = timer.ElapsedMilliseconds,
            ComplexSaMethodResults = complexSaMethodResults,
            IsHidingDetected = isHidingDetected
        };
    }
}
