using System;
using System.Collections.Generic;

namespace Ogu.Extensions.Logging.Timings
{
    /// <summary>
    /// Provides extension methods for the <see cref="Operation"/> class to simplify logging and handling of task outcomes.
    /// </summary>
    public static class OperationExtensions
    {
        /// <summary>
        /// Enriches the resulting log event with the provided exception and skips the exception-handling block, 
        /// allowing the exception to be rethrown.
        /// </summary>
        /// <param name="operation">The operation to enrich with the exception.</param>
        /// <param name="exception">The exception related to the event.</param>
        /// <returns><c>false</c> to indicate that the exception is to be rethrown.</returns>
        /// <remarks>
        /// Example usage:
        /// <code>
        /// using (var op = logger.BeginOperation(...))
        /// {
        ///     try
        ///     {
        ///         ...
        ///         op.Complete();
        ///     }
        ///     catch (Exception e) when (op.SetExceptionAndRethrow(e))
        ///     {
        ///         // This will never be called as the exception is already handled
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static bool SetExceptionAndRethrow(this Operation operation, Exception exception)
        {
            operation.SetException(exception);
            return false;
        }

        /// <summary>
        /// Completes the timed operation and enriches the log context with a key-value pair before completion.
        /// </summary>
        /// <param name="operation">The operation instance to complete.</param>
        /// <param name="enricher">A key-value pair to add to the log context.</param>
        /// <remarks>
        /// Example usage:
        /// <code>
        /// using (var op = logger.BeginOperation(...))
        /// {
        ///     ...
        ///     op.Complete(new KeyValuePair&lt;string, object&gt;("UserId", 1));
        /// }
        /// </code>
        /// </remarks>
        public static void Complete(this Operation operation, KeyValuePair<string, object> enricher)
            => operation.EnrichWith(enricher).Complete();

        /// <summary>
        /// Completes the timed operation and enriches the log context with a collection of key-value pairs before completion.
        /// </summary>
        /// <param name="operation">The operation instance to complete.</param>
        /// <param name="enrichers">A collection of key-value pairs to add to the log context.</param>
        public static void Complete(this Operation operation, IEnumerable<KeyValuePair<string, object>> enrichers)
            => operation.EnrichWith(enrichers).Complete();

        /// <summary>
        /// Abandons the timed operation and enriches the log context with the result property before abandonment.
        /// </summary>
        /// <param name="operation">The operation instance to abandon.</param>
        /// <param name="propertyName">The name of the result property to add to the log context.</param>
        /// <param name="value">The value of the result property to add to the log context.</param>
        /// <remarks>
        /// Example usage:
        /// <code>
        /// using (var op = logger.BeginOperation(...))
        /// {
        ///     ...
        ///     op.Abandon("UserId", 1);
        /// }
        /// </code>
        /// </remarks>
        public static void Abandon(this Operation operation, string propertyName, object value)
            => operation.EnrichWith(propertyName, value).Abandon();

        /// <summary>
        /// Abandons the timed operation and enriches the log context with a key-value pair before abandonment.
        /// </summary>
        /// <param name="operation">The operation instance to abandon.</param>
        /// <param name="enricher">A key-value pair to add to the log context before abandonment.</param>
        /// <remarks>
        /// Example usage:
        /// <code>
        /// using (var op = logger.BeginOperation(...))
        /// {
        ///     ...
        ///     op.Abandon(new KeyValuePair&lt;string, object&gt;("UserId", 1));
        /// }
        /// </code>
        /// </remarks>
        public static void Abandon(this Operation operation, KeyValuePair<string, object> enricher)
            => operation.EnrichWith(enricher).Abandon();

        /// <summary>
        /// Abandons the operation and enriches the log context with a collection of key-value pairs before abandonment.
        /// </summary>
        /// <param name="operation">The operation instance to abandon.</param>
        /// <param name="enrichers">A collection of key-value pairs to add to the log context before abandonment.</param>
        public static void Abandon(this Operation operation, IEnumerable<KeyValuePair<string, object>> enrichers)
            => operation.EnrichWith(enrichers).Abandon();

        /// <summary>
        /// Abandons the timed operation with an included exception.
        /// </summary>
        /// <param name="operation">The operation instance to abandon.</param>
        /// <param name="exception">The exception to set for the operation before abandonment.</param>
        /// <remarks>
        /// Example usage:
        /// <code>
        /// using (var op = logger.BeginOperation(...))
        /// {
        ///     try
        ///     {
        ///         ...
        ///     }
        ///     catch(Exception ex)
        ///     {
        ///         op.Abandon(ex);
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static void Abandon(this Operation operation, Exception exception)
            => operation.SetException(exception).Abandon();
    }
}