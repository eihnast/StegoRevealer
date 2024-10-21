namespace StegoRevealer.Utils.CorrelationAnalyser.Entities;

public class CorrelationValues
{
    public int N {  get; set; }
    public double Alpha { get; set; }
    public double Spearman { get; set; }
    public double TValue { get; set; }
    public double PValue { get; set; }
    public double CriticalTValue { get; set; }
}
