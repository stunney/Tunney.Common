using System;

namespace Tunney.Common.Data
{
    public interface ISemaphoreCreator : ISemaphoreSetter
    {
        void Create();
        void Create(DateTimeOffset _originalStampValue);
    }
}