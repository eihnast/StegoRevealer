using System.Diagnostics;

namespace StegoRevealer.Common;

public class Logger : IDisposable
{
    private const long MaxLogTime = 1000 * 60 * 60 * 2; // 2 часа

    public void Log(string message, Constants.LogMessageType type, bool lineFeed = true) => LogInner(message, type, lineFeed);


    private Stopwatch _timer = Stopwatch.StartNew();

    public string LogName { get; private set; } = string.Empty;
    public string FileSuffix { get; set; } = string.Empty;


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

    public Logger(string? fileSuffix = null)
    {
        FileSuffix = string.IsNullOrEmpty(fileSuffix) ? string.Empty : fileSuffix;
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

            LogInner($"This log is a continuation of the '{oldLogName}' (cut-off log time is {MaxLogTime} ms)", Constants.LogMessageType.Info, lineFeed: true);
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

    private void LogInner(string message, Constants.LogMessageType type, bool lineFeed)
    {
        if (!Configurator.Settings.IsLoggingEnabled)
            return;

        string dateTimePrefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff ");
        string typePrefix = Constants.LogPrefixDictionary[type];
        WriteStringInLog(dateTimePrefix + typePrefix + message, lineFeed);
    }
}
