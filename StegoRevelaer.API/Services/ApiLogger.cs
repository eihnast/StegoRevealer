using StegoRevealer.Common;

namespace StegoRevelaer.API.Services;

public class ApiLogger : IDisposable, ILoggerService
{
    // Описание синглтона
    private static ApiLogger? _instance;
    private static readonly object _lock = new object();
    public static ApiLogger Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                        _instance = new ApiLogger();
                }
            }
            return _instance;
        }
    }

    private Logger Logger;
    private ApiLogger()
    {
        Logger = new Logger(fileSuffix: "API");
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
    ~ApiLogger() => Dispose(false);

    public void Log(Constants.LogMessageType messageType, string message)
    {
        Instance.Logger.Log(message, messageType);
        DefaultLogger?.Log(MapLogLevel(messageType), message);
    }

    public static void LogInfo(string message) => Instance.Log(Constants.LogMessageType.Info, message);
    public static void LogWarning(string message) => Instance.Log(Constants.LogMessageType.Warning, message);
    public static void LogError(string message) => Instance.Log(Constants.LogMessageType.Error, message);


    public static ILogger? DefaultLogger { get; set; }

    private static LogLevel MapLogLevel(Constants.LogMessageType messageType) => messageType switch
    {
        Constants.LogMessageType.Info => LogLevel.Information,
        Constants.LogMessageType.Warning => LogLevel.Warning,
        Constants.LogMessageType.Error => LogLevel.Error,
        _ => LogLevel.Information
    };
}
