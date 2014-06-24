using System;
using System.Collections.Generic;
using System.Text;

using Quartz;

using Tunney.Common.Data;
using Tunney.Common.IoC;
using Tunney.Common.Jobs;
using Tunney.Common.Notifiers.Email;
using Tunney.Common.Statistics;

namespace Tunney.Common.Scheduling.Listeners
{
    [Serializable]
    public class DependecyInjectingJobListener : Tunney.Common.Scheduling.InjectingJobListener
    {
        protected readonly IDataHelper m_dataHelper;
        protected readonly ISemaphoreFactory m_semaphoreFactory;

        public DependecyInjectingJobListener(ILogger _logger, IIoCContainer _iocContainer, IEmailer _emailNotifier, IStatisticsDataAccess _statsWriter, IDataHelper _dataHelper, ISemaphoreFactory _semaphoreFactory)
            : base(_logger, _iocContainer, _emailNotifier, _statsWriter)
        {
            if (null == _dataHelper) throw new ArgumentNullException(@"_dataHelper");
            if (null == _semaphoreFactory) throw new ArgumentNullException(@"_semaphoreFactory");

            m_dataHelper = _dataHelper;
            m_semaphoreFactory = _semaphoreFactory;
        }

        public override void JobToBeExecuted(Quartz.JobExecutionContext context)
        {
            IJob job = (IJob)context.JobInstance;

            if (job is IDataHelperHolder)
            {
                ((IDataHelperHolder)job).DataHelper = m_dataHelper;
            }

            if (job is ISemaphoreFactoryReader)
            {
                ((ISemaphoreFactoryReader)job).SemaphoreFactory = m_semaphoreFactory;
            }

            base.JobToBeExecuted(context);
        }
    }
}