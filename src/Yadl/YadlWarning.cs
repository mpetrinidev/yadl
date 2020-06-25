using System;

namespace Microsoft.Extensions.Logging
{
    public static class YadlWarning
    {
        private static EventId _eventId = new EventId(1, "Default Warning");

        private static readonly Action<ILogger, string, Exception> _warning = LoggerMessage.Define<string>(
            LogLevel.Warning,
            _eventId,
            "{Message}"
        );
        
        public static void LogWarningHighPerf(this ILogger logger, string message) => _warning(logger, message, null);
        public static void LogWarningHighPerf(this ILogger logger, EventId eventId, string message)
        {
            _eventId = eventId;
            _warning(logger, message, null);
        }

        public static void LogWarningHighPerf(this ILogger logger, Exception exception, string message) =>
            _warning(logger, message, exception);

        public static void LogWarningHighPerf(this ILogger logger, EventId eventId, Exception exception, string message)
        {
            _eventId = eventId;
            _warning(logger, message, exception);
        }    
    }
}