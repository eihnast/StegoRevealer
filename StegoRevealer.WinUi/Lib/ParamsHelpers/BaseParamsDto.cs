using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;

namespace StegoRevealer.WinUi.Lib.ParamsHelpers
{
    public abstract class BaseParamsDto<T> where T : class
    {
        public abstract void FillParameters(ref T parameters);
    }
}
