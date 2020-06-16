using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Logging
{
    public static class YadlLog
    {
        private static EventId _eventId = new EventId(1, "Default");

        private static readonly Action<ILogger, string, Exception> _information = LoggerMessage.Define<string>(
            LogLevel.Information,
            _eventId,
            "{Message}"
        );

        public static void Information(this ILogger logger, string message) => _information(logger, message, null);
        public static void Information(this ILogger logger, EventId eventId, string message)
        {
            _eventId = eventId;
            _information(logger, message, null);
        }

        public static void Information(this ILogger logger, Exception exception, string message) =>
            _information(logger, message, exception);

        public static void Information(this ILogger logger, EventId eventId, Exception exception, string message)
        {
            _eventId = eventId;
            _information(logger, message, exception);
        }
    }
}