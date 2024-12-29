using Microsoft.Extensions.Logging;
using System;

namespace Ogu.Extensions.Logging.Timings
{
    /// <summary>
    ///     Provides extension methods for <see cref="ILogger"/> to simplify the creation and logging of timed operations.
    /// </summary>
    public static class LoggerOperationExtensions
    {
        /// <summary>
        ///     Starts a timed operation and logs the time taken for the operation to complete. 
        ///     The operation is completed automatically with an <see cref="Operation"/>.
        /// </summary>
        /// <param name="logger">The logger instance used to log the operation.</param>
        /// <param name="messageTemplate">The message template for the log entry.</param>
        /// <param name="args">Arguments to format the message template.</param>
        /// <returns>An <see cref="IDisposable"/> instance that, when disposed, logs the completion of the operation.</returns>
        public static IDisposable TimeOperation(this ILogger logger, string messageTemplate, params object[] args)
        {
            return new Operation(logger, messageTemplate, args, CompletionBehaviour.Complete, LogLevel.Information, LogLevel.Warning);
        }

        /// <summary>
        ///     Begins a new timed operation and logs its start. The operation will be marked as abandoned if not completed.
        /// </summary>
        /// <param name="logger">The logger instance used to log the operation.</param>
        /// <param name="messageTemplate">The message template for the log entry.</param>
        /// <param name="args">Arguments to format the message template.</param>
        /// <returns>An <see cref="Operation"/> instance that represents the ongoing operation.</returns>
        public static Operation BeginOperation(this ILogger logger, string messageTemplate, params object[] args)
        {
            return new Operation(logger, messageTemplate, args, CompletionBehaviour.Abandon, LogLevel.Information, LogLevel.Warning);
        }

        /// <summary>
        ///     Starts a timed operation with specified completion and abandonment log levels, and optional warning threshold.
        /// </summary>
        /// <param name="logger">The logger instance used to log the operation.</param>
        /// <param name="completion">The log level for the operation's completion message.</param>
        /// <param name="abandonment">The log level for the operation's abandonment message (optional).</param>
        /// <param name="warningThreshold">A threshold for warning log level, if the operation takes too long (optional).</param>
        /// <returns>A <see cref="LevelledOperation"/> instance representing the operation with the specified log levels. 
        /// Returns <see cref="LevelledOperation.None"/> if neither log level is enabled on the logger.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is null.</exception>
        public static LevelledOperation OperationAt(this ILogger logger, LogLevel completion, LogLevel? abandonment = null, TimeSpan? warningThreshold = null)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

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