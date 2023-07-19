using Microsoft.Extensions.Logging;
using System;

namespace Ogu.Extensions.Logging.Timings
{
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

        LevelledOperation(Operation cachedResult)
        {
            _cachedResult = cachedResult ?? throw new ArgumentNullException(nameof(cachedResult));
        }

        internal static LevelledOperation None { get; } = new LevelledOperation(
            new Operation(
                new LoggerFactory().CreateLogger(""),
                "", Array.Empty<object>(),
                CompletionBehaviour.Silent,
                LogLevel.Critical,
                LogLevel.Critical));

        public Operation Begin(string messageTemplate, params object[] args)
        {
            return _cachedResult ?? new Operation(_logger, messageTemplate, args, CompletionBehaviour.Abandon, _completion, _abandonment, _warningThreshold);
        }

        public IDisposable Time(string messageTemplate, params object[] args)
        {
            return _cachedResult ?? new Operation(_logger, messageTemplate, args, CompletionBehaviour.Complete, _completion, _abandonment, _warningThreshold);
        }
    }
}