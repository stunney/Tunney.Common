using System;

namespace Tunney.Common.Data
{
    public interface ISemaphoreSetter : ISemaphoreChecker
    {
        void Set(DateTimeOffset _value);
    }
}