namespace StegoRevealer.StegoCore.Logger
{
    public class LogMessage
    {
        private readonly string _message;
        public string Message { get { return _message; } }

        private readonly LogMessageType _type;
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
}
