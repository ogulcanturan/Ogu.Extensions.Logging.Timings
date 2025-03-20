using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ogu.Extensions.Logging.Timings
{
    /// <summary>
    /// Represents an operation that is tracked for logging purposes. 
    /// It logs the start and completion of tasks along with their duration.
    /// The operation can be <c>completed</c>, <c>abandoned</c>, or <c>cancelled</c>, and supports enriching the log context with additional properties.
    /// </summary>
    public class Operation : IDisposable
    {
        /// <summary>
        /// Specifies the properties that can be included in the log for an operation.
        /// </summary>
        public enum Properties
        {
            /// <summary>
            /// The elapsed time of the operation in milliseconds.
            /// </summary>
            Elapsed,

            /// <summary>
            /// The outcome of the operation (e.g., completed or abandoned).
            /// </summary>
            Outcome,

            /// <summary>
            /// The unique identifier associated with the operation, added to the log context.
            /// </summary>
            OperationId
        }

        private const string OutcomeCompleted = "completed", OutcomeAbandoned = "abandoned";
        private static readonly double StopwatchToTimeSpanTicks = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;

        private readonly ILogger _target;
        private readonly string _messageTemplate;
        private readonly object[] _args;
        private readonly long _start;
        private long? _stop;

        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private CompletionBehaviour _completionBehaviour;
        private readonly LogLevel _completionLevel;
        private readonly LogLevel _abandonmentLevel;
        private readonly TimeSpan? _warningThreshold;
        private Exception _exception;

        internal Operation(ILogger target, string messageTemplate, object[] args,
            CompletionBehaviour completionBehaviour, LogLevel completionLevel, LogLevel abandonmentLevel,
            TimeSpan? warningThreshold = null)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _messageTemplate = messageTemplate ?? throw new ArgumentNullException(nameof(messageTemplate));
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _completionBehaviour = completionBehaviour;
            _completionLevel = completionLevel;
            _abandonmentLevel = abandonmentLevel;
            _warningThreshold = warningThreshold;
            _disposables.Add(_target.BeginScope(new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>(nameof(Properties.OperationId), Guid.NewGuid()) }));
            _start = GetTimestamp();
        }

        private static long GetTimestamp()
        {
            return unchecked((long)(Stopwatch.GetTimestamp() / StopwatchToTimeSpanTicks));
        }

        /// <summary>
        /// Gets the elapsed time of the operation.
        /// Returns the time span since the operation started, or zero if the operation has not yet been completed.
        /// </summary>
        public TimeSpan Elapsed
        {
            get
            {
                var stop = _stop ?? GetTimestamp();
                var elapsedTicks = stop - _start;

                return elapsedTicks < 0 ? TimeSpan.Zero : TimeSpan.FromTicks(elapsedTicks);
            }
        }

        /// <summary>
        /// Marks the timed operation as completed and logs the outcome with the default log level.
        /// </summary>
        public void Complete()
        {
            if (_completionBehaviour == CompletionBehaviour.Silent)
            {
                return;
            }

            Write(_target, _completionLevel, OutcomeCompleted);
        }

        /// <summary>
        /// Marks the timed operation as completed and logs the outcome with the specified log level.
        /// </summary>
        /// <param name="level">The log level to use for logging the completion outcome.</param>
        public void Complete(LogLevel level)
        {
            if (_completionBehaviour == CompletionBehaviour.Silent)
            {
                return;
            }

            Write(_target, level, OutcomeCompleted);
        }

        /// <summary>
        /// Marks the timed operation as completed and logs the outcome with an additional property.
        /// </summary>
        /// <param name="propertyName">The name of the result property.</param>
        /// <param name="value">The value of the result property.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="propertyName"/> is null.</exception>
        public void Complete(string propertyName, object value)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (_completionBehaviour == CompletionBehaviour.Silent)
            {
                return;
            }

            _disposables.Add(_target.BeginScope(new Dictionary<string, object> { { propertyName, value } }));

            Write(_target, _completionLevel, OutcomeCompleted);
        }

        /// <summary>
        /// Marks the timed operation as completed and logs the outcome with an additional property and a specified log level.
        /// </summary>
        /// <param name="propertyName">The name of the result property.</param>
        /// <param name="value">The value of the result property.</param>
        /// <param name="level">The log level to use for logging the completion outcome.</param>
        public void Complete(string propertyName, object value, LogLevel level)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (_completionBehaviour == CompletionBehaviour.Silent)
                return;

            _disposables.Add(_target.BeginScope(new Dictionary<string, object> { { propertyName, value } }));

            Write(_target, level, OutcomeCompleted);
        }

        /// <summary>
        /// Marks the timed operation as abandoned and logs the abandonment outcome.
        /// </summary>
        public void Abandon()
        {
            if (_completionBehaviour == CompletionBehaviour.Silent)
                return;

            Write(_target, _abandonmentLevel, OutcomeAbandoned);
        }

        /// <summary>
        /// Cancels the timed operation, suppressing any further logging. The operation will not be completed or abandoned.
        /// </summary>
        public void Cancel()
        {
            _completionBehaviour = CompletionBehaviour.Silent;
            DisposeContext();
        }

        /// <summary>
        /// Disposes of the timed operation, logging the outcome based on the completion behavior (complete or abandon).
        /// </summary>
        public void Dispose()
        {
            switch (_completionBehaviour)
            {
                case CompletionBehaviour.Silent:
                    break;

                case CompletionBehaviour.Abandon:
                    Write(_target, _abandonmentLevel, OutcomeAbandoned);
                    break;

                case CompletionBehaviour.Complete:
                    Write(_target, _completionLevel, OutcomeCompleted);
                    break;

                default:
                    throw new InvalidOperationException("Unknown underlying state value");
            }

            DisposeContext();
        }

        private void DisposeContext()
        {
            _disposables.ForEach(d => d?.Dispose());
        }

        private void Write(ILogger target, LogLevel level, string outcome)
        {
            _stop = _stop ?? GetTimestamp();

            _completionBehaviour = CompletionBehaviour.Silent;

            var elapsed = Elapsed.TotalMilliseconds;

            level = elapsed > _warningThreshold?.TotalMilliseconds && level < LogLevel.Warning
                ? LogLevel.Warning
                : level;

            if (_target.IsEnabled(level))
                target.Log(level, exception: _exception, $"{_messageTemplate} {{{nameof(Properties.Outcome)}}} in {{{nameof(Properties.Elapsed)}:0.0000}}ms", _args.Concat(new object[] { outcome, elapsed }).ToArray());

            DisposeContext();
        }

        /// <summary>
        /// Enriches the timed operation with additional properties in the log context.
        /// </summary>
        /// <param name="enrichers">The properties to add to the log context.</param>
        /// <returns>The current operation instance for method chaining.</returns>
        public Operation EnrichWith(params KeyValuePair<string, object>[] enrichers)
        {
            _disposables.Add(_target.BeginScope(enrichers));
            return this;
        }

        /// <summary>
        /// Enriches the timed operation with additional properties in the log context.
        /// </summary>
        /// <param name="enrichers">The properties to add to the log context.</param>
        /// <returns>The current operation instance for method chaining.</returns>
        public Operation EnrichWith(IEnumerable<KeyValuePair<string, object>> enrichers)
        {
            _disposables.Add(_target.BeginScope(enrichers));
            return this;
        }

        /// <summary>
        /// Enriches the timed operation with a single additional property in the log context.
        /// </summary>
        /// <param name="propertyName">The name of the property to add to the log context.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>The current operation instance for method chaining.</returns>
        public Operation EnrichWith(string propertyName, object value) => EnrichWith(new KeyValuePair<string, object>(propertyName, value));

        /// <summary>
        /// Sets an exception to be logged for the timed operation.
        /// </summary>
        /// <param name="exception">The exception associated with the operation.</param>
        /// <returns>The current operation instance for method chaining.</returns>
        public Operation SetException(Exception exception)
        {
            _exception = exception;
            return this;
        }
    }
}