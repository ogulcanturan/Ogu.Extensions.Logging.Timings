using Microsoft.Extensions.Logging;
using System;

namespace Ogu.Extensions.Logging.Timings
{
    public static class LoggerOperationExtensions
    {
        public static IDisposable TimeOperation(this ILogger logger, string messageTemplate, params object[] args)
        {
            return new Operation(logger, messageTemplate, args, CompletionBehaviour.Complete, LogLevel.Information, LogLevel.Warning);
        }

        public static Operation BeginOperation(this ILogger logger, string messageTemplate, params object[] args)
        {
            return new Operation(logger, messageTemplate, args, CompletionBehaviour.Abandon, LogLevel.Information, LogLevel.Warning);
        }

        public static LevelledOperation OperationAt(this ILogger logger, LogLevel completion, LogLevel? abandonment = null, TimeSpan? warningThreshold = null)
        {
            if (logger == null) 
                throw new ArgumentNullException(nameof(logger));

            var appliedAbandonment = abandonment ?? completion;
            if (!logger.IsEnabled(completion) &&
                (appliedAbandonment == completion || !logger.IsEnabled(appliedAbandonment)))
            {
                return LevelledOperation.None;
            }

            return new LevelledOperation(logger, completion, appliedAbandonment, warningThreshold);
        }
    }
}