namespace StegoRevealer.StegoCore.StegoMethods;

public interface IExtractResult : ILoggedStegoResult
{
    public string? GetResultData();
}
