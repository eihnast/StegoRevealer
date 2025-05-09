﻿using System.Diagnostics;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;

/// <summary>
/// Анализатор статистических метрик изображения -
/// показателей визуального искажения
/// </summary>
public class StatmAnalyser
{
    /// <summary>
    /// Параметры метода
    /// </summary>
    public StatmParameters Params { get; set; }

    /// <summary>
    /// Внутренний метод-прослойка для записи в лог
    /// </summary>
    private Action<string> _writeToLog = (string str) => new string(str);


    public StatmAnalyser(ImageHandler image)
    {
        Params = new StatmParameters(image);
    }

    public StatmAnalyser(StatmParameters parameters)
    {
        Params = parameters;
    }


    /// <summary>
    /// Запуск анализа
    /// </summary>
    /// <param name="verboseLog">Вести подробный лог</param>
    public StatmResult Analyse(bool verboseLog = false)
    {
        var timer = Stopwatch.StartNew();

        var result = new StatmResult();
        _writeToLog = result.Log;
        _writeToLog($"Started quality characteristics calculation for image '{Params.Image.ImgName}'");

        var noiseCalcTask = new Task<double>(() => new NoiseCalculator(Params).CalcNoiseLevel(NoiseCalculator.NoiseCalculationMethod.Method2));  // Шум
        var sharpnessCalcTask = new Task<double>(() => new SharpnessCalculator(Params).CalcSharpness());  // Резкость
        var blurCalcTask = new Task<double>(() => new BlurCalculator(Params).CalcBlur());  // Размытость
        var contrastCalcTask = new Task<double>(() => new ContrastCalculator(Params).CalcContrast());  // Контраст
        var entropyCalcTask = new Task<EntropyData>(() => new EntropyCalculator(Params).CalcEntropy());  // Энтропия

        noiseCalcTask.Start();
        sharpnessCalcTask.Start();
        blurCalcTask.Start();
        contrastCalcTask.Start();
        entropyCalcTask.Start();

        noiseCalcTask.Wait();
        sharpnessCalcTask.Wait();
        blurCalcTask.Wait();
        contrastCalcTask.Wait();
        entropyCalcTask.Wait();

        result.NoiseValue = noiseCalcTask.Result;
        result.SharpnessValue = sharpnessCalcTask.Result;
        result.BlurValue = blurCalcTask.Result;
        result.ContrastValue = contrastCalcTask.Result;
        result.EntropyValues = entropyCalcTask.Result;

        timer.Stop();
        _writeToLog($"Quality characteristics calculation ended for {timer.ElapsedMilliseconds} ms");

        result.ElapsedTime = timer.ElapsedMilliseconds;
        return result;
    }
}
