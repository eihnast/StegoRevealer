using StegoRevealer.Common;

namespace StegoRevealer.Common.ConsoleInterface.Tools;

public class LoggerHandler
{
    private readonly List<Action> _logActions = new();
    private readonly object _lock = new object();

    public LoggerHandler() { }

    public void LogInfo(string message)
    {
        _logActions.Add(() => CommonLogger.LogInfo(message));
    }

    public void LogWarning(string message)
    {
        _logActions.Add(() => CommonLogger.LogWarning(message));
    }

    public void LogError(string message)
    {
        _logActions.Add(() => CommonLogger.LogError(message));
    }

    public void Flush()
    {
        lock (_lock)
        {
            foreach (var logAction in _logActions)
                logAction();
        }

        _logActions.Clear();
    }
}
