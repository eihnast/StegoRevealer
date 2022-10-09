namespace StegoRevealer.StegoCore.Logger
{
    public abstract class LoggedResult
    {
        // Логирование
        private List<LogMessage> _logRecords = new();
        public List<LogMessage> LogRecords { get { return _logRecords; } }


        public void Write(string msg, LogMessageType type = LogMessageType.Info) =>
            _logRecords.Add(new LogMessage(msg, type));


        public void Log(string msg) => Write(msg, LogMessageType.Info);
        public void Warning(string msg) => Write(msg, LogMessageType.Warning);
        public void Error(string msg)
        {
            _errorsNum++;
            Write(msg, LogMessageType.Error);
        }


        private int _errorsNum = 0;
        public bool HasErrors { get { return _errorsNum > 0; } }
        public List<LogMessage> GetErrors()
        {
            List<LogMessage> errors = new List<LogMessage>();
            foreach (LogMessage logRecord in _logRecords)
                if (logRecord.Type == LogMessageType.Error)
                    errors.Add(logRecord);
            return errors;
        }
    }
}
