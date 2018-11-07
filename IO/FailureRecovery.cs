using System;
using System.Collections.Generic;
using System.Threading;

namespace IO
{
    public static class FailureRecovery
    {

        public static T CallWithRetry<T>(
            Func<T> action, out int retryCounter,
            int maxAttemptCount = 5)
        {
            var exceptions = new List<Exception>();

            for (retryCounter = 0; retryCounter < maxAttemptCount; retryCounter++)
            {
                var rand = new Random();
                try
                {
                    if (retryCounter > 0)
                    {
                        Thread.Sleep(rand.Next(100, 2000));
                    }
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}