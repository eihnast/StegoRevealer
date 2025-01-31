using StegoRevealer_StegoCore_TrainingModule;

namespace StegoRevealer.StegoCore.DecisionModule;

public static class ModelsMapper
{
    public static DecisionModel.ModelInput SaResultToDeicisionInputModel(SteganalysisResults saResult)
    {
        return new DecisionModel.ModelInput
        {
            ChiSquareVolume = (float)saResult.ChiSquareHorizontalVolume,
            RsVolume = (float)saResult.RsVolume,
            KzhaThreshold = (float)saResult.KzhaHorizontalThreshold,
            KzhaMessageVolume = (float)saResult.KzhaHorizontalMessageBitVolume,
            NoiseValue = (float)saResult.NoiseValue,
            SharpnessValue = (float)saResult.SharpnessValue,
            BlurValue = (float)saResult.BlurValue,
            ContrastValue = (float)saResult.ContrastValue,
            EntropyShennonValue = (float)saResult.EntropyShennonValue,
            EntropyRenyiValue = (float)saResult.EntropyRenyiValue
        };
    }
}
