using System;

namespace Tunney.Common.Data
{
    public interface ISemaphoreFactory
    {
        T GetSemaphore<T>(IDataHelper _dataHelper, string _semaphoreName) where T : ISemaphoreChecker;
    }
}