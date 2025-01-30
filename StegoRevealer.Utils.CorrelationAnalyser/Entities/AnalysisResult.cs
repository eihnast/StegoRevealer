namespace StegoRevealer.Utils.CorrelationAnalyser.Entities;

public class AnalysisResult
{
    public string Filename { get; set; } = string.Empty;
    public double ChiSqrValue { get; set; }
    public double RsValue { get; set; }
    public double NoiseValue { get; set; }
    public double SharpnessValue { get; set; }
    public double BlurValue { get; set; }
    public double ContrastValue { get; set; }
    public double EntropyShennonValue { get; set; }
    public double EntropyVaidaValue { get; set; }
    public double EntropyTsallisValue { get; set; }
    public double EntropyRenyiValue { get; set; }
    public double EntropyHavardValue { get; set; }
    public Dictionary<double, double> EntropyRenyiTestValues { get; set; } = new();
}
