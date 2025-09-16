using Microsoft.Extensions.Logging;

public sealed class SrLoggerProvider : ILoggerProvider
{
    private readonly Action<string> _push;
    public SrLoggerProvider(Action<string> push) => _push = push;

    public ILogger CreateLogger(string categoryName) => new SrLogger(categoryName, _push);

    public void Dispose() { }

    private sealed class SrLogger : ILogger
    {
        private readonly string _category;
        private readonly Action<string> _push;

        public SrLogger(string category, Action<string> push)
        {
            _category = category;
            _push = push;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true; // отфильтруешь уровни в настройках

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                                Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var msg = formatter(state, exception);
            if (string.IsNullOrEmpty(msg)) return;

            var line = $"[{DateTime.Now:HH:mm:ss}] {logLevel,-11} {_category}: {msg}";
            if (exception != null) line += Environment.NewLine + exception;

            _push(line);
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
