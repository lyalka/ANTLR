using System;
using Microsoft.Extensions.Logging;

namespace ScriptExecutor.Client.Infrastructure
{
    public class DelagatingLogger<T> : ILogger<T>
    {
        private readonly Action<string, string> _logWriter;

        public DelagatingLogger(Action<string, string> logWriter)
        {
            _logWriter = logWriter;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var msg = formatter(state, exception);
            _logWriter(msg, logLevel.ToString());
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Disposable();
        }

        class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
