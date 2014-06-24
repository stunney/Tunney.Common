using System;

namespace Tunney.Common.Data.Semaphores
{
    public abstract class ASemaphoreSetter : ASemaphoreChecker, ISemaphoreSetter
    {
        protected ASemaphoreSetter(string _semaphoreKey)
            : base(_semaphoreKey)
        {
        }

        #region ISemaphoreSetter Members

        public abstract void Set(DateTimeOffset _value);
        
        #endregion
    }
}