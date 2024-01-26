using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace StegoRevealer.UI.Tools;

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

    private Logger()
    {
        try
        {
            string tempDir = Path.GetTempPath();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string? localDirPath = Directory.GetParent(Path.GetTempPath())?.Parent?.FullName;
                if (!string.IsNullOrEmpty(localDirPath))
                    tempDir = localDirPath;
            }

            var srTempPath = Path.Combine(tempDir, "StegoRevealer");
            if (!Path.Exists(srTempPath))
                Directory.CreateDirectory(srTempPath);
            tempDir = srTempPath;

            string logName = $"sr_log_{DateTime.Now:yy-MM-dd-HH-mm-ss}.log";
            string logPath = Path.Combine(tempDir, logName);

            _logWriter = new StreamWriter(logPath, append: false);
        }
        catch
        {
            _logWriter = null;
        }
    }

    private async void WriteStringInLogAsync(string message, bool lineFeed)
    {
        if (_logWriter is not null)
        {
            try
            {
                if (lineFeed)
                    await _logWriter.WriteLineAsync(message);
                else
                    await _logWriter.WriteAsync(message);
                await _logWriter.FlushAsync();
            }
            catch { }
        }
    }
    private void WriteStringInLog(string message, bool lineFeed)
    {
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
        string dateTimePrefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff ");
        string typePrefix = PrefixDictionary[type];
        WriteStringInLog(dateTimePrefix + typePrefix + message, lineFeed);
    }
}
