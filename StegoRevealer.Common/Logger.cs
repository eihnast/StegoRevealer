using System.Diagnostics;

namespace StegoRevealer.Common;

public class Logger : IDisposable
{
    // Описание синглтона
    private static Logger? _instance;
    private static readonly object _lock = new object();
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

    private static Stopwatch _timer = Stopwatch.StartNew();
    private const long MaxLogTime = 1000 * 60 * 60 * 2; // 2 часа

    public static string LogName { get; private set; } = string.Empty;

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

    public static string FileSuffix { get; set; } = string.Empty;


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

            LogName = $"sr_log{(string.IsNullOrEmpty(FileSuffix) ? "" : $"_{FileSuffix}")}_{DateTime.Now:yy-MM-dd-HH-mm-ss}.log";
            string logPath = Path.Combine(tempDir, LogName);

            _logWriter = new StreamWriter(logPath, append: false);
            _logWasCreated = true;

            _timer = Stopwatch.StartNew();
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
        
        if (_timer.ElapsedMilliseconds >= MaxLogTime)
        {
            _timer.Restart();

            string oldLogName = LogName;
            CloseLog();
            CheckSettingAndTryCreateLog();

            LogInner($"This log is a continuation of the '{oldLogName}' (cut-off log time is {MaxLogTime} ms)", MessageType.Info, lineFeed: true);
        }

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
            catch
            {
                // Любая ошибка записи в лог на этом этапе игнорируется, т.к. записать в лог её нельзя, но это недостаточно критично для завершения программы
            }
        }
    }

    private void CloseLog()
    {
        _logWriter?.Close();
        _logWriter?.Dispose();
        _logWriter = null;
        _logWasCreated = false;
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
