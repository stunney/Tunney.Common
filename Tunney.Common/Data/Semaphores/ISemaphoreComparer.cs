using System;

namespace Tunney.Common.Data.Semaphores
{
    public interface ISemaphoreComparer
    {
        TimeSpan Diff(string _semaphoreNameA, string _semaphoreNameB);
        TimeSpan Diff(ISemaphoreChecker _semaphoreA, ISemaphoreChecker _semaphoreB);
    }
}