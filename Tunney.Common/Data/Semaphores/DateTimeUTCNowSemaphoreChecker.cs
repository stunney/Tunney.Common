using System;

namespace Tunney.Common.Data.Semaphores
{
    [Serializable]
    public class DateTimeUTCNowSemaphoreChecker : ASemaphoreChecker
    {
        public DateTimeUTCNowSemaphoreChecker()
            : base("DateTime.UtcNow")
        {
        }

        public override DateTimeOffset Check()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}