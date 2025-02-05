using StegoRevealer.StegoCore.AnalysisMethods;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.CommonLib;

public static class CoreHelper
{
    /// <summary>
    /// Создаёт словарь с null-(default-)значениями указанного типа по методам стегоанализа
    /// </summary>
    public static Dictionary<AnalysisMethod, T?> CreateValuesByAnalysisMethodDictionary<T>()
    {
        var dict = new Dictionary<AnalysisMethod, T?>();
        foreach (AnalysisMethod method in Enum.GetValues(typeof(AnalysisMethod)))
            dict.Add(method, default);
        return dict;
    }

    public static int GetContainerFrequencyVolume(ImageHandler img) => (img.Height / 8) * (img.Width / 8);
}
