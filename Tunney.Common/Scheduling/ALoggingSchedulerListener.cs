using System;
using Quartz;
using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling
{
    public abstract class ALoggingSchedulerListener : ISchedulerListener
    {
        private readonly ILogger m_logger;

        protected ALoggingSchedulerListener(ILogger _logger)
        {
            if (null == _logger)
            {
                throw new ArgumentNullException(@"_logger");
            }
            m_logger = _logger;
        }

        public ILogger Logger
        {
            get { return m_logger; }
        }

        #region ISchedulerListener Members

        public virtual void JobScheduled(Trigger trigger)
        {
            Logger.INFO_Format(@"JOB SCHEDULED.  Job Name/Group: {0}/{1}, Trigger Name/Group: {2}/{3}.  Next Fire Time (UTC): {4}", trigger.JobName, trigger.JobGroup, trigger.Name, trigger.Group, trigger.GetNextFireTimeUtc());
        }

        public virtual void JobUnscheduled(string triggerName, string triggerGroup)
        {
            Logger.INFO_Format(@"JOB UN-SCHEDULED.  Trigger Name/Group: {0}/{1}", triggerName, triggerGroup);
        }

        public virtual void JobsPaused(string jobName, string jobGroup)
        {
            Logger.INFO_Format(@"JOB PAUSED.  Job Name/Group: {0}/{1}", jobName, jobGroup);
        }

        public virtual void JobsResumed(string jobName, string jobGroup)
        {
            Logger.INFO_Format(@"JOB RESUMED.  Job Name/Group: {0}/{1}", jobName, jobGroup);
        }

        public virtual void SchedulerError(string msg, SchedulerException cause)
        {
            if (cause is JobPersistenceException)
            {
                Logger.INFO(cause.Message);
            }
            else
            {
                Logger.WARN(cause);
            }
        }

        public virtual void SchedulerShutdown()
        {
            Logger.INFO_Format(@"Scheduler shut down at {0}.", DateTime.UtcNow);
        }

        public virtual void TriggerFinalized(Trigger trigger)
        {
            Logger.DEBUG_Format(@"TRIGGER FINALIZED.  Job Name/Group: {0}/{1}, Trigger Name/Group: {2}/{3}", trigger.JobName, trigger.JobGroup, trigger.Name, trigger.Group);
        }

        public virtual void TriggersPaused(string triggerName, string triggerGroup)
        {
            Logger.INFO_Format(@"TRIGGER PAUSED.  Trigger Name/Group: {0}/{1}.", triggerName, triggerGroup);
        }

        public virtual void TriggersResumed(string triggerName, string triggerGroup)
        {
            Logger.INFO_Format(@"TRIGGER RESUMED.  Trigger Name/Group: {0}/{1}.", triggerName, triggerGroup);
        }

        #endregion
    }
}