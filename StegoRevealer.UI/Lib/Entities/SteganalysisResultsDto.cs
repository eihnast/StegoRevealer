using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.SamplePairAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;
using StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.Logger;
using System.Collections.Generic;
using System.Linq;

namespace StegoRevealer.UI.Lib.Entities;

/// <summary>
/// Данные результатов стегоанализа, передаваемые во View и для вывода
/// </summary>
public class SteganalysisResultsDto
{
    // CSA
    public SaMethodExecutionState MethodChiSqrState { get; private set; } = SaMethodExecutionState.NotExecuted;
    public double ChiSqrMessageRelativeVolume { get; private set; } = 0.0;
    public List<string> ChiSqrErrors { get; private set; } = new();

    // RS
    public SaMethodExecutionState MethodRsState { get; private set; } = SaMethodExecutionState.NotExecuted;
    public double RsMessageRelativeVolume { get; private set; } = 0.0;
    public List<string> RsErrors { get; private set; } = new();

    // SPA
    public SaMethodExecutionState MethodSpaState { get; private set; } = SaMethodExecutionState.NotExecuted;
    public double SpaMessageRelativeVolume { get; private set; } = 0.0;
    public List<string> SpaErrors { get; private set; } = new();

    // FAN (HCF-COM)
    public SaMethodExecutionState MethodFanState { get; private set; } = SaMethodExecutionState.NotExecuted;
    public bool IsFanHidingDetected { get; private set; } = false;
    public double? FanMahalanobisDistance { get; private set; } = 0.0;
    public List<string> FanErrors { get; private set; } = new();

    // ZCA
    public SaMethodExecutionState MethodZcaState { get; private set; } = SaMethodExecutionState.NotExecuted;
    public bool IsZcaHidingDetected { get; private set; } = false;
    public List<string> ZcaErrors { get; private set; } = new();

    // KZHA
    public SaMethodExecutionState MethodKzhaState { get; private set; } = SaMethodExecutionState.NotExecuted;
    public bool KzhaSuspiciousIntervalIsFound { get; private set; } = false;
    public double KzhaThreshold { get; private set; } = 0.0;
    public ScIndexPair? KzhaCoefficients { get; private set; } = null;
    public int KzhaMessageBitsVolume { get; private set; } = 0;
    public string? KzhaExtractedData { get; private set; } = null;
    public (int leftInd, int rightInd)? KzhaSuspiciousInterval { get; private set; } = null;
    public List<string> KzhaErrors { get; private set; } = new();

    // STATM
    public SaMethodExecutionState StatmCalcState { get; private set; } = SaMethodExecutionState.NotExecuted;
    public double StatmNoiseValue { get; private set; } = 0.0;
    public double StatmSharpnessValue { get; private set; } = 0.0;
    public double StatmBlurValue { get; private set; } = 0.0;
    public double StatmContrastValue { get; private set; } = 0.0;
    public double StatmEntropyShennonValue { get; private set; } = 0.0;
    public double StatmEntropyRenyiValue { get; private set; } = 0.0;
    public List<string> StatmErrors { get; private set; } = new();

    // COMPLEX
    public SaMethodExecutionState ComplexMethodState { get; private set; } = SaMethodExecutionState.NotExecuted;
    public bool IsHidingDetected { get; private set; }
    public double DecisionPobability { get; private set; } = 0.0;
    public List<string> ComplexMethodErrors { get; private set; } = new();

    // 
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
            MethodChiSqrState = GetMethodState(chiRes);
            ChiSqrMessageRelativeVolume = chiRes.MessageRelativeVolume;
            ChiSqrErrors = GetErrorsEntries(chiRes);
        }

        if (rsRes is not null)
        {
            MethodRsState = GetMethodState(rsRes);
            RsMessageRelativeVolume = rsRes.MessageRelativeVolume;
            RsErrors = GetErrorsEntries(rsRes);
        }

        if (spaRes is not null)
        {
            MethodSpaState = GetMethodState(spaRes);
            SpaMessageRelativeVolume = spaRes.MessageRelativeVolume;
            SpaErrors = GetErrorsEntries(spaRes);
        }

        if (fanRes is not null)
        {
            MethodFanState = GetMethodState(fanRes);
            IsFanHidingDetected = fanRes.IsHidingDetected;
            FanMahalanobisDistance = fanRes.MahalanobisDistance;
            FanErrors = GetErrorsEntries(fanRes);
        }

        if (zcaRes is not null)
        {
            MethodZcaState = GetMethodState(zcaRes);
            IsZcaHidingDetected = zcaRes.IsHidingDetected;
            ZcaErrors = GetErrorsEntries(zcaRes);
        }

        if (kzhaRes is not null)
        {
            MethodKzhaState = GetMethodState(kzhaRes);
            KzhaSuspiciousIntervalIsFound = kzhaRes.SuspiciousIntervalIsFound;
            KzhaThreshold = kzhaRes.Threshold;
            KzhaCoefficients = kzhaRes.Coefficients;
            KzhaMessageBitsVolume = kzhaRes.MessageBitsVolume;
            KzhaExtractedData = kzhaRes.ExtractedData;
            KzhaSuspiciousInterval = kzhaRes.SuspiciousInterval;
            KzhaErrors = GetErrorsEntries(kzhaRes);
        }

        if (statmRes is not null)
        {
            StatmCalcState = GetMethodState(statmRes);
            StatmNoiseValue = statmRes.NoiseValue;
            StatmSharpnessValue = statmRes.SharpnessValue;
            StatmBlurValue = statmRes.BlurValue;
            StatmContrastValue = statmRes.ContrastValue;
            StatmEntropyShennonValue = statmRes.EntropyValues.Shennon;
            StatmEntropyRenyiValue = statmRes.EntropyValues.Renyi;
            StatmErrors = GetErrorsEntries(statmRes);
        }

        if (complexSaResult is not null)
        {
            ComplexMethodState = GetMethodState(complexSaResult);
            IsHidingDetected = complexSaResult.IsHidingDetected;
            DecisionPobability = complexSaResult.DecisionProbability;
            ComplexMethodErrors = GetErrorsEntries(complexSaResult);
        }
    }

    private static SaMethodExecutionState GetMethodState(LoggedResult result) => result.HasErrors
                ? (result.MethodSuccessful ? SaMethodExecutionState.WithErrors : SaMethodExecutionState.FatalError)
                : SaMethodExecutionState.Executed;

    private static List<string> GetErrorsEntries(LoggedResult result) => result.GetErrors().Select(x => x.ToString()).ToList();
}
