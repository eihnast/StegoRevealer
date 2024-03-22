namespace StegoRevealer.Utils.DataPreparer.Entities;

public class ImageAnalysisData
{
    public double ChiSquareVolume { get; set; }
    public double RsVolume { get; set; }
    public double KzhaThreshold { get; set; }
    public int KzhaMessageBitVolume { get; set; }
    public double ChiSquareVolume_Vertical { get; set; }
    public double KzhaThreshold_Vertical { get; set; }
    public int KzhaMessageBitVolume_Vertical { get; set; }
    public double NoiseValue { get; set; }
    public double SharpnessValue { get; set; }
    public double BlurValue { get; set; }
    public double ContrastValue { get; set; }
    public double EntropyShennonValue { get; set; }
    public double EntropyVaidaValue { get; set; }
    public double EntropyTsallisValue { get; set; }
    public double EntropyRenyiValue { get; set; }
    public double EntropyHavardValue { get; set; }
    public int PixelsNum { get; set; }
    public int DataWasHided { get; set; }
}
