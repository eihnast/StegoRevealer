using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.StegoMethods.Lsb
{
    public class LsbExtractResult : LoggedResult, IExtractResult
    {
        public string? ResultData { get; set; } = null;
        public string? GetResultData() => ResultData;
        public LoggedResult AsLog()
        {
            return this;
        }
    }
}
