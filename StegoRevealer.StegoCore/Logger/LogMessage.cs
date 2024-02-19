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


    public LogMessage(string msg, LogMessageType type = LogMessageType.Info)
    {
        _message = msg;
        _type = type;
    }

    public override string ToString()
    {
        return $"[{_type}] {_message}";
    }
}
