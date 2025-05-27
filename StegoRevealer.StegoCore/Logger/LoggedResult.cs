using System.Text.Json.Serialization;

namespace StegoRevealer.StegoCore.Logger;

/// <summary>
/// Результат работы метода, содержащий внутренние записи лога
/// </summary>
public abstract class LoggedResult
{
    private readonly List<LogMessage> _logRecords = new();  // Записи лога

    /// <summary>
    /// Получение записей лога
    /// </summary>
    [JsonIgnore]
    public List<LogMessage> LogRecords { get { return _logRecords; } }

    /// <summary>
    /// Запись в лог
    /// </summary>
    /// <param name="msg">Сообщение</param>
    /// <param name="type">Тип сообщения</param>
    public void WriteLog(string msg, LogMessageType type = LogMessageType.Info) =>
        _logRecords.Add(new LogMessage(msg, type));


    /// <summary>
    /// Запись информационного сообщения в лог
    /// </summary>
    public void LogInfo(string msg) => WriteLog(msg, LogMessageType.Info);

    /// <summary>
    /// Запись предупреждения в лог
    /// </summary>
    public void LogWarning(string msg) => WriteLog(msg, LogMessageType.Warning);

    /// <summary>
    /// Запись ошибки в лог
    /// </summary>
    public void LogError(string msg)
    {
        _errorsNum++;
        WriteLog(msg, LogMessageType.Error);
    }


    private int _errorsNum = 0;  // Количество ошибок в логе

    /// <summary>
    /// Содержит ли лог сообщения об ошибках
    /// </summary>
    public bool HasErrors { get { return _errorsNum > 0; } }

    /// <summary>
    /// Считать ли метод выполненным (независимо от наличия ошибок в логе)
    /// </summary>
    public bool MethodSuccessful { get; set; } = true;


    /// <summary>
    /// Возвращает записи об ошибках
    /// </summary>
    public List<LogMessage> GetErrors()
    {
        List<LogMessage> errors = new List<LogMessage>();
        foreach (LogMessage logRecord in _logRecords)
            if (logRecord.Type == LogMessageType.Error)
                errors.Add(logRecord);
        return errors;
    }

    public string ToString(int indent = 0) => indent == 0 
        ? string.Join("\n", _logRecords) 
        : string.Join("\n", _logRecords.Select(r => string.Join("", Enumerable.Repeat("\t", indent)) + r.ToString()));
}
