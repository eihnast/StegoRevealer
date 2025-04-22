namespace StegoRevealer.StegoCore.DecisionModule;

public class SaDecisionFeatures
{
    public double ChiSquareHorizontalVolume { get; set; } = 0.0;
    public double ChiSquareVerticalVolume { get; set; } = 0.0;
    public double RsVolume { get; set; } = 0.0;
    public double KzhaHorizontalThreshold { get; set; } = 0.0;
    public int KzhaHorizontalMessageBitVolume { get; set; } = 0;
    public double KzhaVerticalThreshold { get; set; } = 0.0;
    public int KzhaVerticalMessageBitVolume { get; set; } = 0;
    public double NoiseValue { get; set; } = 0.0;
    public double SharpnessValue { get; set; } = 0.0;
    public double BlurValue { get; set; } = 0.0;
    public double ContrastValue { get; set; } = 0.0;
    public double EntropyShennonValue { get; set; } = 0.0;
    public double EntropyRenyiValue { get; set; } = 0.0;
    public int PixelsNumber { get; set; } = 0;
}
