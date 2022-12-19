using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;

namespace StegoRevealer.WinUi.Lib.ParamsHelpers
{
    /// <summary>
    /// Базовый класс DTO параметров для методов стегоанализа
    /// </summary>
    public abstract class BaseParamsDto<T> where T : class
    {
        /// <summary>
        /// Записывает значения из DTO в объект параметров
        /// </summary>
        public abstract void FillParameters(ref T parameters);
    }
}
