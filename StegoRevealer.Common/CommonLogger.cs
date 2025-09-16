namespace StegoRevealer.Common;

public class CommonLogger : IDisposable, ILoggerService
{
    // Описание синглтона
    private static CommonLogger? _instance;
    private static readonly object _lock = new object();
    public static CommonLogger Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                        _instance = new CommonLogger();
                }
            }
            return _instance;
        }
    }

    private Logger Logger;
    private CommonLogger()
    {
        Logger = new Logger();
    }


    private bool _isDisposed = false;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            Logger?.Dispose();
        }

        _isDisposed = true;
    }
    ~CommonLogger() => Dispose(false);

    public void Log(Constants.LogMessageType messageType, string message) => Instance.Logger.Log(message, messageType);

    public static void LogInfo(string message) => Instance.Log(Constants.LogMessageType.Info, message);
    public static void LogWarning(string message) => Instance.Log(Constants.LogMessageType.Warning, message);
    public static void LogError(string message) => Instance.Log(Constants.LogMessageType.Error, message);
}
