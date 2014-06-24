using System;
using Quartz;
using Tunney.Common.IoC;
using Tunney.Common.Scheduling;

namespace Tunney.Common.Scheduling.Listeners
{
    [Serializable]
    public class LookForFailingJobsAndStopSchedulerListener : StdJobListener
    {
        public LookForFailingJobsAndStopSchedulerListener(ILogger _logger)
            : base(_logger)
        {
        }

        #region IJobListener Members

        public override void JobWasExecuted(JobExecutionContext context, JobExecutionException jobException)
        {
            if (null != jobException)
            {
                //context.Scheduler.Shutdown(true);
            }
            base.JobWasExecuted(context, jobException);
        }

        #endregion
    }
}