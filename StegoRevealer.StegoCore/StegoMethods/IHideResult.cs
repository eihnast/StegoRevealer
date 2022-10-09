using StegoRevealer.StegoCore.Logger;

namespace StegoRevealer.StegoCore.StegoMethods
{
    public interface IHideResult : IStegoResult
    {
        public string? GetResultPath();
    }
}
