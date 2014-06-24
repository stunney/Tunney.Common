using System;
using System.Text;

using Quartz;
using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling
{
    [Serializable]
    public class StdJobListener : ALoggingJobListener
    {
        public StdJobListener(ILogger _logger)
            : base(_logger)
        {
        }

        #region IJobListener Members

        public override void JobExecutionVetoed(JobExecutionContext context)
        {
            string message = string.Format("Job was vetoed, details are \r\n{0}", BuildJobDetails(context.JobDetail));
            Logger.INFO(message);
        }

        public override void JobToBeExecuted(JobExecutionContext context)
        {
            string message = string.Format("Job to be executed, details are \r\n{0}", BuildJobDetails(context.JobDetail));
            Logger.INFO(message);
        }

        public override void JobWasExecuted(JobExecutionContext context, JobExecutionException jobException)
        {
            if (null == jobException)
            {
                string message = string.Format("Job executed for {1}, details are \r\n{0}", BuildJobDetails(context.JobDetail), context.JobRunTime);
                Logger.INFO(message);
            }
            else
            {
                string message = string.Format("Job errored out after running for {2}, details are \r\n{0}\r\n{1}", BuildJobDetails(context.JobDetail), jobException, context.JobRunTime);
                Logger.ERROR(message);
            }
        }

        public override string Name
        {
            get
            {
                return GetType().FullName;
            }
        }

        #endregion
    }
}