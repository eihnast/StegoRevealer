using System.Text.Json.Serialization;

namespace StegoRevealer.StegoCore.Logger
{
    /// <summary>
    /// Результат работы метода, содержащий внутренние записи лога
    /// </summary>
    public abstract class LoggedResult
    {
        private List<LogMessage> _logRecords = new();  // Записи лога

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
        public void Write(string msg, LogMessageType type = LogMessageType.Info) =>
            _logRecords.Add(new LogMessage(msg, type));


        /// <summary>
        /// Запись информационного сообщения в лог
        /// </summary>
        public void Log(string msg) => Write(msg, LogMessageType.Info);

        /// <summary>
        /// Запись предупреждения в лог
        /// </summary>
        public void Warning(string msg) => Write(msg, LogMessageType.Warning);

        /// <summary>
        /// Запись ошибки в лог
        /// </summary>
        public void Error(string msg)
        {
            _errorsNum++;
            Write(msg, LogMessageType.Error);
        }


        private int _errorsNum = 0;  // Количество ошибок в логе

        /// <summary>
        /// Содержит ли лог сообщения об ошибках
        /// </summary>
        public bool HasErrors { get { return _errorsNum > 0; } }


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

        public override string ToString() => string.Join("\n", _logRecords);
        public string ToString(int indent = 0) => string.Join("\n", _logRecords.Select(r => string.Join("", Enumerable.Repeat("\t", indent)) + r.ToString()));
    }
}
