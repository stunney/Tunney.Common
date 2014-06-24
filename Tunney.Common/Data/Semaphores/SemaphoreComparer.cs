using System;

namespace Tunney.Common.Data.Semaphores
{
    [Serializable]
    public class SemaphoreComparer : ISemaphoreComparer
    {
        protected readonly ISemaphoreFactory m_semaphoreFactory;
        protected readonly IDataHelper m_dataHelper;

        public SemaphoreComparer(ISemaphoreFactory _semaphoreFactory, IDataHelper _dataHelper)
        {
            if (null == _semaphoreFactory) throw new ArgumentNullException(@"_semaphoreFactory");
            if (null == _dataHelper) throw new ArgumentNullException(@"_dataHelper");
            
            m_semaphoreFactory = _semaphoreFactory;
            m_dataHelper = _dataHelper;
        }

        public virtual TimeSpan Diff(string _semaphoreNameA, string _semaphoreNameB)
        {
            if (string.IsNullOrEmpty(_semaphoreNameA)) throw new ArgumentNullException(@"_semaphoreNameA");
            if (string.IsNullOrEmpty(_semaphoreNameB)) throw new ArgumentNullException(@"_semaphoreNameB");

            ISemaphoreChecker semaphoreA = GetTaskTimeTrackerSemaphore(_semaphoreNameA);
            ISemaphoreChecker semaphoreB = GetTaskTimeTrackerSemaphore(_semaphoreNameB);

            return Diff(semaphoreA, semaphoreB);
        }

        public virtual TimeSpan Diff(ISemaphoreChecker _semaphoreA, ISemaphoreChecker _semaphoreB)
        {
            if (null == _semaphoreA) throw new ArgumentNullException(@"_semaphoreA");
            if (null == _semaphoreB) throw new ArgumentNullException(@"_semaphoreB");

            DateTimeOffset a = _semaphoreA.Check();
            DateTimeOffset b = _semaphoreB.Check();

            TimeSpan diff = (a - b);
            return diff;
        }

        protected virtual ISemaphoreChecker GetTaskTimeTrackerSemaphore(string _semaphoreIoCName)
        {
            ISemaphoreChecker retval = m_semaphoreFactory.GetSemaphore<ISemaphoreChecker>(m_dataHelper, _semaphoreIoCName);
            return retval;
        }
    }
}