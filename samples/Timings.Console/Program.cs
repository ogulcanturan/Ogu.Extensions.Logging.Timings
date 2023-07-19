using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ogu.Extensions.Logging.Timings;
using System;

namespace Timings.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace).AddConsole());
            
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();

            int userId = 1;

            #region Usage 1

            using (logger.TimeOperation("User: {UserId} is saving to database", userId))
            {
                // Do some operations
            }

            #endregion

            #region Usage 2

            using (var op = logger.BeginOperation("User: {UserId} is removing from database.", userId))
            {
                if (true)
                {
                    op.Abandon();
                }

                // Do some operations

                op.Complete("Username", "ogulcanturan");
            }

            #endregion

            #region Usage 3

            using (var op = logger.BeginOperation("You will not see this message on console, because of this statement => 'op.Cancel()'"))
            {
                if (true)
                {
                    op.Cancel();
                }

                // Do some operations

                op.Complete();
            }

            #endregion

            #region Usage 4

            using (var op = logger.BeginOperation("Doing some operations..."))
            {
                try
                {
                    int calculation = int.Parse("You cannot parse this to number!");
                }
                catch (Exception ex)
                {
                    op.SetException(ex);
                }

                op.Complete(); // If you don't call 'op.Complete()' it will call implicitly 'op.Abandon()'
            }

            #endregion

            System.Console.WriteLine("Press something to close app");
            System.Console.ReadKey();
            System.Console.WriteLine();

            #region Usage 5

            using (logger.OperationAt(LogLevel.Trace).Time("App is closing..."))
            {
                // Do some operations
            }

            #endregion
        }
    }
}