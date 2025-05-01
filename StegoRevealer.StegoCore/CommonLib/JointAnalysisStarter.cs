using System.Diagnostics;
using StegoRevealer.StegoCore.AnalysisMethods;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.CommonLib.Entities;

namespace StegoRevealer.StegoCore.CommonLib;

public static class JointAnalysisStarter
{
    public static async Task<JointAnalysisResult> Start(JointAnalysisMethodsParameters parameters)
    {
        var results = CoreHelper.CreateValuesByAnalysisMethodDictionary<ILoggedAnalysisResult>();  // Словарь результатов
        var methodTasks = CoreHelper.CreateValuesByAnalysisMethodDictionary<Task<ILoggedAnalysisResult>>();  // Словарь задач

        var timer = Stopwatch.StartNew();  // Запуск таймера - подсчёт времени работы непосредственно методов стегоанализа

        // Создание задач
        if (parameters.ChiSquareParameters is not null)  // CSA
        {
            var chiSqrMethodAnalyzer = new ChiSquareAnalyser(parameters.ChiSquareParameters);
            methodTasks[AnalysisMethod.ChiSquare] = Task.Run(() => chiSqrMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.RsParameters is not null)  // RS
        {
            var rsMethodAnalyzer = new RsAnalyser(parameters.RsParameters);
            methodTasks[AnalysisMethod.RegularSingular] = Task.Run(() => rsMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.KzhaParameters is not null)  // CKZhA
        {
            var kzhaMethodAnalyzer = new KzhaAnalyser(parameters.KzhaParameters);
            methodTasks[AnalysisMethod.KochZhaoAnalysis] = Task.Run(() => kzhaMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.SpaParameters is not null)  // SPA
        {
            var spaMethodAnalyzer = new SpaAnalyser(parameters.SpaParameters);
            methodTasks[AnalysisMethod.Spa] = Task.Run(() => spaMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.FanParameters is not null)  // FAN
        {
            var fanMethodAnalyzer = new FanAnalyser(parameters.FanParameters);
            methodTasks[AnalysisMethod.Fan] = Task.Run(() => fanMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.ZcaParameters is not null)  // ZCA
        {
            var zcaMethodAnalyzer = new ZcaAnalyser(parameters.ZcaParameters);
            methodTasks[AnalysisMethod.Zca] = Task.Run(() => zcaMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.StatmParameters is not null)  // Statm
        {
            var statmAnalyzer = new StatmAnalyser(parameters.StatmParameters);
            methodTasks[AnalysisMethod.Statm] = Task.Run(() => statmAnalyzer.Analyse() as ILoggedAnalysisResult);
        }
        if (parameters.ComplexSaMethodParameters is not null)  // ComplexMethod
        {
            var complexSaMethodAnalyzer = new ComplexSaMethodAnalyser(parameters.ComplexSaMethodParameters);
            methodTasks[AnalysisMethod.Complex] = Task.Run(() => complexSaMethodAnalyzer.Analyse() as ILoggedAnalysisResult);
        }

        // Ожидание
        foreach (var methodTask in methodTasks)
            if (methodTask.Value is not null)
                await methodTask.Value;

        // Сбор результатов
        foreach (AnalysisMethod method in Enum.GetValues(typeof(AnalysisMethod)))
            results[method] = methodTasks[method]?.Result;

        timer.Stop();  // Остановка таймера

        // Приведение к известным типам результатов
        var chiRes = results[AnalysisMethod.ChiSquare] as ChiSquareResult;
        var rsRes = results[AnalysisMethod.RegularSingular] as RsResult;
        var spaRes = results[AnalysisMethod.Spa] as SpaResult;
        var fanRes = results[AnalysisMethod.Fan] as FanResult;
        var zcaRes = results[AnalysisMethod.Zca] as ZcaResult;
        var kzhaRes = results[AnalysisMethod.KochZhaoAnalysis] as KzhaResult;
        var statmRes = results[AnalysisMethod.Statm] as StatmResult;
        var complexSaRes = results[AnalysisMethod.Complex] as ComplexSaMethodResult;

        return new JointAnalysisResult
        {
            ChiSquareResult = chiRes,
            RsResult = rsRes,
            SpaResult = spaRes,
            FanResult = fanRes,
            ZcaResult = zcaRes,
            KzhaResult = kzhaRes,
            StatmResult = statmRes,
            ComplexSaMethodResults = complexSaRes,
            ElapsedTime = timer.ElapsedMilliseconds
        };
    }
}
