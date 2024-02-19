namespace StegoRevealer.StegoCore.StegoMethods;

public interface IHideResult : ILoggedStegoResult
{
    public string? GetResultPath();
}
