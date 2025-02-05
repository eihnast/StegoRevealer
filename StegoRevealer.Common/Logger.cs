namespace StegoRevealer.Common;

public class Logger : IDisposable
{
    // Описание синглтона
    private static Logger? _instance;
    private static object _lock = new object();
    public static Logger Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    if (_instance is null)
                        _instance = new Logger();
                }
            }
            return _instance;
        }
    }

    public enum MessageType
    {
        Info,
        Warning,
        Error
    }

    private static Dictionary<MessageType, string> PrefixDictionary = new Dictionary<MessageType, string>()
    {
        { MessageType.Info, "[Info] " },
        { MessageType.Warning, "[Warning] " },
        { MessageType.Error, "[Error] " }
    };


    public static void Log(string message, MessageType type, bool lineFeed = true) => Instance.LogInner(message, type, lineFeed);
    public static void LogInfo(string message) => Log(message, MessageType.Info);
    public static void LogWarning(string message) => Log(message, MessageType.Warning);
    public static void LogError(string message) => Log(message, MessageType.Error);


    public static string Separator { get => "------------------------------"; }


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
            _logWriter?.Close();
            _logWriter?.Dispose();
        }

        _isDisposed = true;
    }
    ~Logger() => Dispose(false);


    private StreamWriter? _logWriter;
    public bool CanLog { get => _logWriter is not null; }
    private bool _logWasCreated = false;

    private Logger()
    {
        if (Configurator.Settings.IsLoggingEnabled)
            CreateLogWriter();
    }

    private void CreateLogWriter()
    {
        if (_logWasCreated)
            return;

        try
        {
            string tempDir = Tools.GetOrCreateTempDirPath();

            string logName = $"sr_log_{DateTime.Now:yy-MM-dd-HH-mm-ss}.log";
            string logPath = Path.Combine(tempDir, logName);

            _logWriter = new StreamWriter(logPath, append: false);
            _logWasCreated = true;
        }
        catch
        {
            _logWriter = null;
        }
    }

    private void CheckSettingAndTryCreateLog()
    {
        if (_logWriter is null && Configurator.Settings.IsLoggingEnabled)
            CreateLogWriter();
    }

    private void WriteStringInLog(string message, bool lineFeed)
    {
        if (_logWriter is null)
            CheckSettingAndTryCreateLog();

        if (_logWriter is not null)
        {
            try
            {
                if (lineFeed)
                    _logWriter.WriteLine(message);
                else
                    _logWriter.Write(message);
                _logWriter.Flush();
            }
            catch { }
        }
    }

    private void LogInner(string message, MessageType type, bool lineFeed)
    {
        if (!Configurator.Settings.IsLoggingEnabled)
            return;

        string dateTimePrefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff ");
        string typePrefix = PrefixDictionary[type];
        WriteStringInLog(dateTimePrefix + typePrefix + message, lineFeed);
    }
}
