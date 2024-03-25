namespace StegoRevealer.Utils.DataPreparer.Lib;

public class Logger : IDisposable
{
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


    public void Log(string message, MessageType? type = null, bool raw = false) => LogInner(message, type, raw);
    public void LogInfo(string message) => Log(message, MessageType.Info);
    public void LogWarning(string message) => Log(message, MessageType.Warning);
    public void LogError(string message) => Log(message, MessageType.Error);

    public void LogRawEnumerable<T>(IEnumerable<T> list, bool asColumn = true, Func<T, string>? toString = null)
    {
        var strItems = new List<string>();
        foreach (var item in list)
        {
            string strItem = toString is null ? item?.ToString() ?? string.Empty : toString(item);
            strItems.Add(strItem);
        }

        string output = string.Empty;
        if (asColumn)
            output = string.Join("\n", strItems);
        else
            output = "[ " + string.Join(", ", strItems) + " ]";

        Log(output + "\n", raw: true);
    }

    public void LogSeparator() => LogInfo(Separator);
    public void LogLineFeed() => Log("\n", raw: true);


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
    private string _logFilePath;

    public Logger(string logPath)
    {
        _logFilePath = logPath;
        CreateLogWriter();
    }

    private void CreateLogWriter()
    {
        if (_logWasCreated)
            return;

        try
        {
            _logWriter = new StreamWriter(_logFilePath, append: false);
            _logWasCreated = true;
        }
        catch
        {
            _logWriter = null;
        }
    }

    private void CheckOrTryCreateLog()
    {
        if (_logWriter is null)
            CreateLogWriter();
    }

    private void WriteStringInLog(string message, bool lineFeed)
    {
        if (_logWriter is null)
            CheckOrTryCreateLog();

        if (_logWriter is not null)
        {
            try
            {
                if (lineFeed)
                {
                    Console.WriteLine(message);
                    _logWriter.WriteLine(message);
                }
                else
                {
                    Console.Write(message);
                    _logWriter.Write(message);
                }
                _logWriter.Flush();
            }
            catch { }
        }
    }

    private object _loggerLock = new object();
    private void LogInner(string message, MessageType? type = null, bool raw = false)
    {
        lock (_loggerLock)
        {
            if (raw)
            {
                WriteStringInLog(message, lineFeed: false);
                return;
            }

            string dateTimePrefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ");
            string typePrefix = type is not null ? PrefixDictionary[type.Value] : string.Empty;
            WriteStringInLog(dateTimePrefix + typePrefix + message, !raw);
        }
    }
}
