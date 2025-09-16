namespace StegoRevealer.Common;

public interface ILoggerService
{
    public void Log(Constants.LogMessageType messageType, string message);
}
