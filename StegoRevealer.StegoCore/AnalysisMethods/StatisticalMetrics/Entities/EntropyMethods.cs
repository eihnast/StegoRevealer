namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;

[Flags]
public enum EntropyMethods
{
    Shennon = 1,
    Vaida = 2,
    Tsallis = 4,
    Renyi = 8,
    Havard = 16,
    All = 32
}
