using StegoRevealer.UI.Tools;
using System;
using System.Collections.Generic;

namespace StegoRevealer.UI.Desktop.ConsoleInterface;

public class LoggerHandler
{
    private List<Action> _logActions = new();
    private object _lock = new object();

    public LoggerHandler() { }

    public void LogInfo(string message)
    {
        _logActions.Add(() => Logger.LogInfo(message));
    }

    public void LogWarning(string message)
    {
        _logActions.Add(() => Logger.LogWarning(message));
    }

    public void LogError(string message)
    {
        _logActions.Add(() => Logger.LogError(message));
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
