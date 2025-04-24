using System.Diagnostics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.DecisionModule;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;

namespace StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;

public class ComplexSaMethodAnalyser
{
    private const string MethodName = "Complex Steganalysis Method";

    /// <summary>
    /// Параметры метода
    /// </summary>
    public ComplexSaMethodParameters Params { get; set; }

    /// <summary>
    /// Внутренний метод-прослойка для записи в лог
    /// </summary>
    private Action<string>? _writeToLog = null;


    public ComplexSaMethodAnalyser(ImageHandler image)
    {
        Params = new ComplexSaMethodParameters(image);
    }

    public ComplexSaMethodAnalyser(ComplexSaMethodParameters parameters)
    {
        Params = parameters;
    }

    /// <summary>
    /// Запуск стегоанализа
    /// </summary>
    /// <param name="verboseLog">Вести подробный лог</param>
    public ComplexSaMethodResult Analyse(bool verboseLog = false)
    {
        var result = new ComplexSaMethodResult();
        var timer = Stopwatch.StartNew();  // Запуск таймера - подсчёт времени работы непосредственно методов стегоанализа

        _writeToLog = result.Log;
        _writeToLog($"Started steganalysis by method '{MethodName}' for image '{Params.Image.ImgName}'");

        // Запуск комплексного стегоанализа
        var methodTasks = new List<Task>();

        var chiSqrMethodHorizontalAnalyzer = new ChiSquareAnalyser(Params.Image);
        chiSqrMethodHorizontalAnalyzer.Params.TraverseType = TraverseType.Horizontal;

        var chiSqrMethodVerticalAnalyzer = new ChiSquareAnalyser(Params.Image);
        chiSqrMethodVerticalAnalyzer.Params.TraverseType = TraverseType.Vertical;

        var rsMethodAnalyzer = new RsAnalyser(Params.Image);

        var kzhaMethodHorizontalAnalyzer = new KzhaAnalyser(Params.Image);
        kzhaMethodHorizontalAnalyzer.Params.TraverseType = TraverseType.Horizontal;

        var kzhaMethodVerticalAnalyzer = new KzhaAnalyser(Params.Image);
        kzhaMethodVerticalAnalyzer.Params.TraverseType = TraverseType.Horizontal;

        var statmAnalyzer = new StatmAnalyser(Params.Image);
        statmAnalyzer.Params.EntropyCalcSensitivity = 1.1;

        // Задачи
        methodTasks.Add(Task.Run(() => { 
            try { result.ChiSquareHorizontalResult = chiSqrMethodHorizontalAnalyzer.Analyse(); }
            catch { /* Не обрабатываем */ } }));
        methodTasks.Add(Task.Run(() => { 
            try { result.ChiSquareVerticalResult = chiSqrMethodVerticalAnalyzer.Analyse(); }
            catch { /* Не обрабатываем */ } }));
        methodTasks.Add(Task.Run(() => { 
            try { result.RsResult = rsMethodAnalyzer.Analyse(); }
            catch { /* Не обрабатываем */ } }));
        methodTasks.Add(Task.Run(() => { 
            try { result.KzhaHorizontalResult = kzhaMethodHorizontalAnalyzer.Analyse(); }
            catch { /* Не обрабатываем */ } }));
        methodTasks.Add(Task.Run(() => { 
            try { result.KzhaVerticalResult = kzhaMethodVerticalAnalyzer.Analyse(); }
            catch { /* Не обрабатываем */ } }));
        methodTasks.Add(Task.Run(() => { 
            try { result.StatmResult = statmAnalyzer.Analyse(); }
            catch { /* Не обрабатываем */ } }));

        // Ожидание
        Task.WaitAll(methodTasks);

        timer.Stop();  // Остановка таймера
        _writeToLog($"Steganalysis by method '{MethodName}' ended for {timer.ElapsedMilliseconds} ms");

        return ProcessResults(result, timer);
    }

    private ComplexSaMethodResult ProcessResults(ComplexSaMethodResult complexSaMethodResults, Stopwatch timer)
    {
        int pixelsNum = Params.Image.Width * Params.Image.Height;

        // Формирование вывода
        var featuresValues = new SaDecisionFeatures
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
            PixelsNumber = pixelsNum
        };

        var decisionResult = SteganalysisDecision.Calculate(featuresValues);

        complexSaMethodResults.IsHidingDetected = decisionResult.IsHidingDetected;
        complexSaMethodResults.DecisionProbability = decisionResult.Probability;

        complexSaMethodResults.PixelsNum = pixelsNum;
        complexSaMethodResults.ElapsedTime = timer.ElapsedMilliseconds;

        return complexSaMethodResults;
    }
}
