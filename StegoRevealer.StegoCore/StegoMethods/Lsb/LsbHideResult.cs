using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.StegoMethods.Lsb
{
    public class LsbHideResult : LoggedResult, IHideResult
    {
        public string? Path { get; set; } = null;
        public string? GetResultPath() => Path;
        public LoggedResult AsLog()
        {
            return this;
        }
    }
}
