using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    public class KochZhaoExtractResult : LoggedResult, IExtractResult
    {
        public string? ResultData { get; set; } = null;
        public string? GetResultData() => ResultData;
        public LoggedResult AsLog()
        {
            return this;
        }
    }
}
