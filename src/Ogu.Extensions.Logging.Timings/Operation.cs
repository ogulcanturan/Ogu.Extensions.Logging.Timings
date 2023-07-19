using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ogu.Extensions.Logging.Timings
{
    public class Operation : IDisposable
    {
        public enum Properties
        {
            Elapsed,
            Outcome,
            OperationId
        };

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

        public TimeSpan Elapsed
        {
            get
            {
                var stop = _stop ?? GetTimestamp();
                var elapsedTicks = stop - _start;

                if (elapsedTicks < 0)
                {
                    return TimeSpan.Zero;
                }

                return TimeSpan.FromTicks(elapsedTicks);
            }
        }

        public void Complete()
        {
            if (_completionBehaviour == CompletionBehaviour.Silent)
                return;

            Write(_target, _completionLevel, OutcomeCompleted);
        }

        public void Complete(string resultPropertyName, object result)
        {
            if (resultPropertyName == null)
                throw new ArgumentNullException(nameof(resultPropertyName));

            if (_completionBehaviour == CompletionBehaviour.Silent)
                return;

            _disposables.Add(_target.BeginScope(new Dictionary<string, object> { { resultPropertyName, result } }));

            Write(_target, _completionLevel, OutcomeCompleted);
        }

        public void Abandon()
        {
            if (_completionBehaviour == CompletionBehaviour.Silent)
                return;

            Write(_target, _abandonmentLevel, OutcomeAbandoned);
        }

        public void Cancel()
        {
            _completionBehaviour = CompletionBehaviour.Silent;
            DisposeContext();
        }

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

        public Operation EnrichWith(params KeyValuePair<string, object>[] enrichers)
        {
            _disposables.Add(_target.BeginScope(enrichers));
            return this;
        }

        public Operation EnrichWith(IEnumerable<KeyValuePair<string, object>> enrichers)
        {
            _disposables.Add(_target.BeginScope(enrichers));
            return this;
        }

        public Operation EnrichWith(string propertyName, object value) => EnrichWith(new KeyValuePair<string, object>(propertyName, value));

        public Operation SetException(Exception exception)
        {
            _exception = exception;
            return this;
        }
    }
}