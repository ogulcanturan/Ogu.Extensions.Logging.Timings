using System;
using System.Collections.Generic;

namespace Ogu.Extensions.Logging.Timings
{
    public static class OperationExtensions
    {
        public static bool SetExceptionAndRethrow(this Operation operation, Exception exception)
        {
            operation.SetException(exception);
            return false;
        }

        public static void Complete(this Operation operation, KeyValuePair<string, object> enricher)
            => operation.EnrichWith(enricher).Complete();

        public static void Complete(this Operation operation, IEnumerable<KeyValuePair<string, object>> enrichers)
            => operation.EnrichWith(enrichers).Complete();

        public static void Abandon(this Operation operation, string resultPropertyName, object result)
            => operation.EnrichWith(resultPropertyName, result).Abandon();

        public static void Abandon(this Operation operation, KeyValuePair<string, object> enricher)
            => operation.EnrichWith(enricher).Abandon();

        public static void Abandon(this Operation operation, IEnumerable<KeyValuePair<string, object>> enrichers)
            => operation.EnrichWith(enrichers).Abandon();

        public static void Abandon(this Operation operation, Exception exception)
            => operation.SetException(exception).Abandon();
    }
}