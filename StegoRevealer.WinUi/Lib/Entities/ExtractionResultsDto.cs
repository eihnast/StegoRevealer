using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using System.Security.Principal;

namespace StegoRevealer.WinUi.Lib.Entities
{
    /// <summary>
    /// Данные результатов извлечения, передаваемые во View и для вывода
    /// </summary>
    public class ExtractionResultsDto
    {
        public string ExtractedMessage { get; set; } = string.Empty;

        public long ElapsedTime { get; set; } = 0;
    }
}
