using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.CommonLib.ScTypes;

namespace StegoRevealer.UI.Lib.Entities;

/// <summary>
/// Данные результатов стегоанализа, передаваемые во View и для вывода
/// </summary>
public class SteganalysisResultsDto
{
    public bool IsMethodChiSqrExecuted { get; private set; } = false;

    public double ChiSqrMessageRelativeVolume { get; private set; } = 0.0;

    public bool IsMethodRsExecuted { get; private set; } = false;

    public double RsMessageRelativeVolume { get; private set; } = 0.0;

    public bool IsMethodSpaExecuted { get; private set; } = false;

    public double SpaMessageRelativeVolume { get; private set; } = 0.0;

    public bool IsMethodFanExecuted { get; private set; } = false;

    public bool IsFanHidingDetected { get; private set; } = false;

    public double? FanMahalanobisDistance { get; private set; } = 0.0;

    public bool IsMethodZcaExecuted { get; private set; } = false;

    public bool IsZcaHidingDetected { get; private set; } = false;

    public bool IsMethodKzhaExecuted { get; private set; } = false;

    public bool KzhaSuspiciousIntervalIsFound { get; private set; } = false;

    public double KzhaThreshold { get; private set; } = 0.0;

    public ScIndexPair? KzhaCoefficients { get; private set; } = null;

    public int KzhaMessageBitsVolume { get; private set; } = 0;

    public string? KzhaExtractedData { get; private set; } = null;

    public (int leftInd, int rightInd)? KzhaSuspiciousInterval { get; private set; } = null;

    public double StatmNoiseValue { get; private set; } = 0.0;

    public double StatmSharpnessValue { get; private set; } = 0.0;

    public double StatmBlurValue { get; private set; } = 0.0;

    public double StatmContrastValue { get; private set; } = 0.0;

    public double StatmEntropyShennonValue { get; private set; } = 0.0;

    public double StatmEntropyRenyiValue { get; private set; } = 0.0;

    public bool IsComplexMethodExecuted { get; private set; } = false;

    public bool IsHidingDetected { get; private set; }

    public double DecisionPobability { get; private set; } = 0.0;


    public long ElapsedTime { get; private set; } = 0;


    /// <summary>
    /// Заполняет DTO результатами стегоанализа<br/>
    /// Если результат по методу передан равным null, будет считаться, что метод не исполнялся
    /// </summary>
    public SteganalysisResultsDto(ChiSquareResult? chiRes = null, RsResult? rsRes = null, SpaResult? spaRes = null, FanResult? fanRes = null, KzhaResult? kzhaRes = null,
        ZcaResult? zcaRes = null, StatmResult? statmRes = null, ComplexSaMethodResult? complexSaResult = null, long? elapsedTime = null)
    {
        if (elapsedTime is not null)
            ElapsedTime = elapsedTime.Value;

        if (chiRes is not null)
        {
            IsMethodChiSqrExecuted = true;
            ChiSqrMessageRelativeVolume = chiRes.MessageRelativeVolume;
        }

        if (rsRes is not null)
        {
            IsMethodRsExecuted = true;
            RsMessageRelativeVolume = rsRes.MessageRelativeVolume;
        }

        if (spaRes is not null)
        {
            IsMethodSpaExecuted = true;
            SpaMessageRelativeVolume = spaRes.MessageRelativeVolume;
        }

        if (fanRes is not null)
        {
            IsMethodFanExecuted = true;
            IsFanHidingDetected = fanRes.IsHidingDetected;
            FanMahalanobisDistance = fanRes.MahalanobisDistance;
        }

        if (zcaRes is not null)
        {
            IsMethodZcaExecuted = true;
            IsZcaHidingDetected = zcaRes.IsHidingDetected;
        }

        if (kzhaRes is not null)
        {
            IsMethodKzhaExecuted = true;
            KzhaSuspiciousIntervalIsFound = kzhaRes.SuspiciousIntervalIsFound;
            KzhaThreshold = kzhaRes.Threshold;
            KzhaCoefficients = kzhaRes.Coefficients;
            KzhaMessageBitsVolume = kzhaRes.MessageBitsVolume;
            KzhaExtractedData = kzhaRes.ExtractedData;
            KzhaSuspiciousInterval = kzhaRes.SuspiciousInterval;
        }

        if (statmRes is not null)
        {
            StatmNoiseValue = statmRes.NoiseValue;
            StatmSharpnessValue = statmRes.SharpnessValue;
            StatmBlurValue = statmRes.BlurValue;
            StatmContrastValue = statmRes.ContrastValue;
            StatmEntropyShennonValue = statmRes.EntropyValues.Shennon;
            StatmEntropyRenyiValue = statmRes.EntropyValues.Renyi;
        }

        if (complexSaResult is not null)
        {
            IsComplexMethodExecuted = true;
            IsHidingDetected = complexSaResult.IsHidingDetected;
            DecisionPobability = complexSaResult.DecisionProbability;
        }
    }
}
