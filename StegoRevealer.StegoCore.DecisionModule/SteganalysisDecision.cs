namespace StegoRevealer.StegoCore.DecisionModule;

public static class SteganalysisDecision
{
    public static SteganalysisDecisionResult Calculate(SaDecisionFeatures features)
    {
        var predict = DecisionModel_ComplexSa.Predict(ModelsMapper.SaResultToDeicisionInputModel(features));
        return new SteganalysisDecisionResult
        {
            IsHidingDetected = predict.PredictedLabel,
            Probability = predict.Probability
        };
    }
}
