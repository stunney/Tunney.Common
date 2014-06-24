using System;

namespace Tunney.Common.Scheduling.ThreadPools
{
    [Serializable]
    public class AutoAffinityThreadPool : Quartz.Simpl.SimpleThreadPool
    {
        protected override System.Collections.IList CreateWorkerThreads(int threadCount)
        {
            return base.CreateWorkerThreads(ThreadCount);
        }
        public AutoAffinityThreadPool()
            : base(Environment.ProcessorCount, System.Threading.ThreadPriority.Normal)
        {
        }

        public new int ThreadCount
        {
            get { return Environment.ProcessorCount; }
            set { }
        }
    }
}