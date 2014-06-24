using System;
using System.Collections.Generic;
using Quartz;
using Tunney.Common.IoC;
using Tunney.Common.Notifiers.Email;

namespace Tunney.Common.Scheduling
{
    [Serializable]
    public class StdSchedulerStarter : ASchedulerStarter
    {
        public StdSchedulerStarter(ISchedulerFactory _schedulerFactory,
                                    IList<IJobScheduler> _jobSchedulers,
                                    IList<IJobListener> _jobListeners,
                                    IList<ISchedulerListener> _schedulerListeners,                                   
                                    IEmailer _emailer)
            : base(_schedulerFactory, _jobSchedulers, _jobListeners, _schedulerListeners, _emailer)
        {
        }
    }
}