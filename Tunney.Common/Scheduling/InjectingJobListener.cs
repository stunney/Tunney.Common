using System;
using Quartz;
using Tunney.Common.IoC;
using Tunney.Common.Notifiers.Email;
using Tunney.Common.Notifiers;
using Tunney.Common.Statistics;

namespace Tunney.Common.Scheduling
{
    [Serializable]
    public class InjectingJobListener : ALoggingJobListener
    {
        protected readonly IIoCContainer m_iocContainer;
        protected readonly IEmailer m_emailNotifier;
        protected readonly IStatisticsDataAccess m_statsWriter;

        protected readonly Guid m_listenerId = Guid.NewGuid();

        public InjectingJobListener(ILogger _logger, IIoCContainer _iocContainer, IEmailer _emailNotifier, IStatisticsDataAccess _statsWriter)
            : base(_logger)
        {
            if (null == _iocContainer) throw new ArgumentNullException(@"_iocContainer");
            if (null == _emailNotifier) throw new ArgumentNullException(@"_emailNotifier");
            if (null == _statsWriter) throw new ArgumentNullException(@"_statsWriter");

            m_iocContainer = _iocContainer;
            m_emailNotifier = _emailNotifier;
            m_statsWriter = _statsWriter;
        }

        public override void JobToBeExecuted(Quartz.JobExecutionContext context)
        {
            try
            {
                IJob jobInstance = context.JobInstance;
                if (null != jobInstance)
                {
                    if (jobInstance is ILogWriter)
                    {
                        ((ILogWriter)jobInstance).Logger = Logger;
                    }

                    if (jobInstance is IContainerUser)
                    {
                        ((IContainerUser)jobInstance).Container = m_iocContainer;
                    }

                    if (jobInstance is IEmailSender)
                    {
                        ((IEmailSender)jobInstance).EmailNotifier = m_emailNotifier;
                    }

                    if (jobInstance is IStatisticsLogger)
                    {
                        ((IStatisticsLogger)jobInstance).Stats = m_statsWriter;
                    }
                }
            }
            finally
            {
                //base.JobToBeExecuted(context);
                //Nothing here.
            }
        }

        public override void JobExecutionVetoed(JobExecutionContext context)
        {            
        }

        public override void JobWasExecuted(JobExecutionContext context, JobExecutionException jobException)
        {            
        }

        public override string Name
        {
            get { return string.Format(@"InjectingJobListener_{0}", m_listenerId); }
        }
    }
}