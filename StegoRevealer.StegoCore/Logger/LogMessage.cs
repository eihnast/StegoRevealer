namespace StegoRevealer.StegoCore.Logger;

/// <summary>
/// Сообщение лога
/// </summary>
public class LogMessage
{
    private readonly string _message;  // Сообщение

    /// <summary>
    /// Сообщение
    /// </summary>
    public string Message { get { return _message; } }


    private readonly LogMessageType _type;  // Тип сообщения

    /// <summary>
    /// Тип сообщения
    /// </summary>
    public LogMessageType Type { get { return _type; } }


    private readonly DateTime _dateTime;  // Время записи

    /// <summary>
    /// Сообщение
    /// </summary>
    public DateTime DateTime { get { return _dateTime; } }


    public LogMessage(string msg, LogMessageType type = LogMessageType.Info, DateTime? dt = null)
    {
        _message = msg;
        _type = type;
        _dateTime = dt ?? DateTime.Now;
    }

    public override string ToString()
    {
        return $"{_dateTime:yy-MM-dd-HH-mm-ss} [{_type}] {_message}";
    }
}
