using System;
using Microsoft.Extensions.Logging;

namespace Core.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogFullError(this ILogger logger, Exception ex)
        {
            logger.Error(ex.FullMessage());
        }

        public static void Error(this ILogger logger, string msg, Exception ex)
        {
            logger.LogError(new EventId(0), ex, msg);
        }

        public static void Error(this ILogger logger, string msg)
        {
            logger. LogError(new EventId(0), msg);
        }

        public static void Info(this ILogger logger, string msg)
        {
            logger.LogInformation(msg);
        }
    }
}
