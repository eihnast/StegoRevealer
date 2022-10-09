using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    public class KochZhaoHideResult : LoggedResult, IHideResult
    {
        public string? Path { get; set; } = null;
        public string? GetResultPath() => Path;
        public LoggedResult AsLog()
        {
            return this;
        }
    }
}
