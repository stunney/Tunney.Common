using System;

namespace Tunney.Common.Data
{
    public interface ISemaphoreChecker
    {
        DateTimeOffset Check();

        string Name { get; }
    }
}