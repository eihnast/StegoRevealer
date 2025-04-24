namespace StegoRevealer.StegoCore.DecisionModule;

public static class ModelsMapper
{
    public static DecisionModel_ComplexSa.ModelInput SaResultToDeicisionInputModel(SaDecisionFeatures saResult)
    {
        return new DecisionModel_ComplexSa.ModelInput
        {
            ChiSqrHorizontalRelativeVolume = (float)saResult.ChiSquareHorizontalVolume,
            ChiSqrVerticalRelativeVolume = (float)saResult.ChiSquareHorizontalVolume,
            RsRelativeVolume = (float)saResult.RsVolume,
            KzhaHorizontalThreshold = (float)saResult.KzhaHorizontalThreshold,
            KzhaHorizontalBitsVolume = (float)saResult.KzhaHorizontalMessageBitVolume,
            KzhaVerticalThreshold = (float)saResult.KzhaVerticalThreshold,
            KzhaVerticalBitsVolume = (float)saResult.KzhaVerticalMessageBitVolume,
            Noise = (float)saResult.NoiseValue,
            Sharpness = (float)saResult.SharpnessValue,
            Blur = (float)saResult.BlurValue,
            Contrast = (float)saResult.ContrastValue,
            EntropyShennon = (float)saResult.EntropyShennonValue,
            EntropyRenyi11 = (float)saResult.EntropyRenyiValue,
            PixelsNum = (float)saResult.PixelsNumber
        };
    }
}
