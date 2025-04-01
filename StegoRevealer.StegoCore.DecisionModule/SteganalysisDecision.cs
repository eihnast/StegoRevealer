namespace StegoRevealer.StegoCore.DecisionModule;

public static class SteganalysisDecision
{
    public static bool Calculate(SteganalysisResults saResult)
    {
        bool isHided = DecisionModel_ComplexSa.Predict(ModelsMapper.SaResultToDeicisionInputModel(saResult)).PredictedLabel;
        return isHided;
    }
}
