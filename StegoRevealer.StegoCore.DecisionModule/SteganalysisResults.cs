namespace StegoRevealer.StegoCore.DecisionModule;

public class SteganalysisResults
{
    public double ChiSquareVolume { get; set; } = 0.0;
    public double RsVolume { get; set; } = 0.0;
    public double KzhaThreshold { get; set; } = 0.0;
    public double KzhaMessageVolume { get; set; } = 0.0;
    public double NoiseValue { get; set; } = 0.0;
    public double SharpnessValue { get; set; } = 0.0;
    public double BlurValue { get; set; } = 0.0;
    public double ContrastValue { get; set; } = 0.0;
    public double EntropyShennonValue { get; set; } = 0.0;
    public double EntropyRenyiValue { get; set; } = 0.0;
}
