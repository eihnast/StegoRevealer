using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.DecisionModule;
using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Diagnostics;

namespace StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;

public class ComplexAnalysisStarter
{
    public async static Task<ComplexAnalysisResults> Start(ComplexAnalysisParameters parameters)
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

        // Ожидание
        foreach (var methodTask in methodTasks)
            if (methodTask.Value is not null)  // methodTask.Value?.Wait();
                await methodTask.Value;

        // Сбор результатов
        foreach (AnalysisMethod method in Enum.GetValues(typeof(AnalysisMethod)))
            results[method] = methodTasks[method]?.Result;

        timer.Stop();  // Остановка таймера

        bool decisionCanBeCalculated = parameters.Image is not null && parameters.ChiSquareParameters is not null && 
            parameters.RsParameters is not null && parameters.KzhaParameters is not null;
        return ProcessResults(parameters.Image, results, timer, decisionCanBeCalculated);
    }

    private static ComplexAnalysisResults ProcessResults(
        ImageHandler? image, Dictionary<AnalysisMethod, ILoggedAnalysisResult?> results, Stopwatch timer, bool decisionCanBeCalculated)
    {
        // Приведение к известным типам результатов
        var chiRes = results[AnalysisMethod.ChiSquare] as ChiSquareResult;
        var rsRes = results[AnalysisMethod.RegularSingular] as RsResult;
        var kzhaRes = results[AnalysisMethod.KochZhaoAnalysis] as KzhaResult;
        var statmRes = results[AnalysisMethod.Statm] as StatmResult;

        // Формирование вывода
        bool? isHidingDetected = null;
        if (decisionCanBeCalculated && chiRes is not null && rsRes is not null && kzhaRes is not null && statmRes is not null)
        {
            var saResult = new SteganalysisResults
            {
                ChiSquareVolume = chiRes.MessageRelativeVolume,
                RsVolume = rsRes.MessageRelativeVolume,
                KzhaThreshold = kzhaRes.Threshold,
                KzhaMessageVolume = kzhaRes.MessageBitsVolume / CoreHelper.GetContainerFrequencyVolume(image!),
                NoiseValue = statmRes.NoiseValue,
                SharpnessValue = statmRes.SharpnessValue,
                BlurValue = statmRes.BlurValue,
                ContrastValue = statmRes.ContrastValue,
                EntropyShennonValue = statmRes.EntropyValues.Shennon,
                EntropyRenyiValue = statmRes.EntropyValues.Renyi
            };

            isHidingDetected = SteganalysisDecision.Calculate(saResult);
        }

        return new ComplexAnalysisResults
        {
            ChiSquareResult = chiRes,
            RsResult = rsRes,
            KzhaResult = kzhaRes,
            StatmResult = statmRes,
            ElapsedTime = timer.ElapsedMilliseconds,
            IsHidingDetected = isHidingDetected
        };
    }
}
