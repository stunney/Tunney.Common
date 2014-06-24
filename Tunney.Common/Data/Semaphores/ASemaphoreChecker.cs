using System;

namespace Tunney.Common.Data.Semaphores
{
    public abstract class ASemaphoreChecker : DataHelperConstants, ISemaphoreChecker
    {
        protected readonly string m_semaphoreKey;

        protected ASemaphoreChecker(string _semaphoreKey)
        {
            if (string.IsNullOrEmpty(_semaphoreKey)) throw new ArgumentNullException(@"_semaphoreKey");
            m_semaphoreKey = _semaphoreKey;
        }

        #region ISemaphoreChecker Members

        public abstract DateTimeOffset Check();

        public virtual string Name { get { return m_semaphoreKey; } }

        #endregion
    }
}