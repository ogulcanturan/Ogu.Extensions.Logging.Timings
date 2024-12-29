using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Ogu.Extensions.Logging.Timings
{
    /// <summary>
    ///     Represents an operation with specified log levels for completion and abandonment.
    ///     Supports time-based operations with optional warning thresholds. If the operation exceeds the 
    ///     warning threshold, the log level may be upgraded to <see cref="Microsoft.Extensions.Logging.LogLevel.Warning"/>.
    /// </summary>
    public class LevelledOperation
    {
        private readonly Operation _cachedResult;

        private readonly ILogger _logger;
        private readonly LogLevel _completion;
        private readonly LogLevel _abandonment;
        private readonly TimeSpan? _warningThreshold;

        internal LevelledOperation(ILogger logger, LogLevel completion, LogLevel abandonment, TimeSpan? warningThreshold = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _completion = completion;
            _abandonment = abandonment;
            _warningThreshold = warningThreshold;
        }

        private LevelledOperation(Operation cachedResult)
        {
            _cachedResult = cachedResult ?? throw new ArgumentNullException(nameof(cachedResult));
        }

        internal static LevelledOperation None { get; } = new LevelledOperation(
            new Operation(
                NullLogger.Instance,
                "", Array.Empty<object>(),
                CompletionBehaviour.Silent,
                LogLevel.Critical,
                LogLevel.Critical));

        /// <summary>
        ///     Begins a new timed operation with the specified message template and arguments. 
        ///     The operation will be marked as abandoned if not completed. If the operation exceeds the 
        ///     warning threshold, the log level may be upgraded to <see cref="LogLevel.Warning"/> if it's below that level.
        /// </summary>
        /// <param name="messageTemplate">The message template for the log entry.</param>
        /// <param name="args">Arguments to format the message template.</param>
        /// <returns>An <see cref="Operation"/> instance representing the ongoing operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageTemplate"/> or <paramref name="args"/> is <c>null</c>.</exception>
        public Operation Begin(string messageTemplate, params object[] args)
        {
            return _cachedResult ?? new Operation(_logger, messageTemplate, args, CompletionBehaviour.Abandon, _completion, _abandonment, _warningThreshold);
        }

        /// <summary>
        ///     Begins a timed operation that logs the completion time when disposed.
        ///     If the operation exceeds the warning threshold, the log level may be upgraded to 
        ///     <see cref="LogLevel.Warning"/> if the current log level is lower.
        /// </summary>
        /// <param name="messageTemplate">The message template for the log entry.</param>
        /// <param name="args">Arguments to format the message template.</param>
        /// <returns>An <see cref="IDisposable"/> instance that, when disposed, logs the completion of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageTemplate"/> or <paramref name="args"/> is <c>null</c>.</exception>
        public IDisposable Time(string messageTemplate, params object[] args)
        {
            return _cachedResult ?? new Operation(_logger, messageTemplate, args, CompletionBehaviour.Complete, _completion, _abandonment, _warningThreshold);
        }
    }
}