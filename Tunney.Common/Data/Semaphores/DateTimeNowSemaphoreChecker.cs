using System;

namespace Tunney.Common.Data.Semaphores
{
    [Serializable]
    public class DateTimeNowSemaphoreChecker : ASemaphoreChecker
    {
        public DateTimeNowSemaphoreChecker()
            : base("DateTime.Now")
        {
        }

        public override DateTimeOffset Check()
        {
            return DateTimeOffset.Now;
        }
    }
}