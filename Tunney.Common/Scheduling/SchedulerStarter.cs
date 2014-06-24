using System;
using System.Collections.Generic;

using Quartz;
using Quartz.Impl.AdoJobStore;

using Tunney.Common.Data;
using Tunney.Common.IoC;
using Tunney.Common.SanityChecks;
using Tunney.Common.Scheduling;

namespace Tunney.Common.Scheduling
{
    public class ScheduleStarter : IScheduleStarter
    {
        private readonly IDataHelper m_dataHelper;

        private readonly IList<Tunney.Common.Scheduling.IJobScheduler> m_jobSchedulers;

        private readonly IList<short> m_locationIDs;
        private readonly bool m_clearAllJobs;

        private readonly IList<IJobListener> m_jobListeners;
        private readonly IList<ISchedulerListener> m_schedulerListeners;

        private readonly IScheduler m_scheduler;

        public ScheduleStarter(IDataHelper _dataHelper, ISchedulerFactory _schedulerFactory, IList<Tunney.Common.Scheduling.IJobScheduler> _jobSchedulers, bool _clearAllJobs, IList<IJobListener> _jobListeners, IList<ISchedulerListener> _schedulerListeners, IList<ISanityChecker> _sanityChecks)
        {
            if (null == _dataHelper) throw new ArgumentNullException(@"_dataHelper");

            if (null == _schedulerFactory)
            {
                throw new ArgumentNullException(@"_schedulerFactory");
            }            

            if (null == _jobSchedulers)
            {
                throw new ArgumentNullException(@"_jobSchedulers");
            }
            if (_sanityChecks == null) throw new ArgumentNullException("_sanityChecks");

            if (null != _jobListeners &&
                0 < _jobListeners.Count)
            {
                //Save thes listeners for registration during Initialize

                m_jobListeners = _jobListeners;
            }

            if (null != _schedulerListeners &&
                0 < _schedulerListeners.Count)
            {
                m_schedulerListeners = _schedulerListeners;
            }

            m_scheduler = _schedulerFactory.GetScheduler();
            m_dataHelper = _dataHelper;
            m_jobSchedulers = _jobSchedulers;

            m_clearAllJobs = _clearAllJobs;

            foreach (ISanityChecker sc in _sanityChecks)
            {
                sc.Check();
            }
        }

        public virtual void Start()
        {
            Initialize();

            m_scheduler.Start();
        }

        public virtual void Stop()
        {
            m_scheduler.Shutdown();
        }

        public virtual void Continue()
        {
            m_scheduler.ResumeAll();
        }

        public virtual void Pause()
        {
            m_scheduler.PauseAll();
        }

        private void ClearAllJobs()
        {
            Dictionary<string, IList<string>> jobNames = new Dictionary<string, IList<string>>(10);

            foreach (string groupName in m_scheduler.JobGroupNames)
            {
                IList<string> triggerNames = m_scheduler.GetTriggerNames(groupName);

                Logger.INFO_Format("{0} Jobs to delete in group {1}.", jobNames.Count, groupName);

                foreach (string tn in triggerNames)
                {
                    m_scheduler.UnscheduleJob(tn, groupName);
                }

                jobNames.Add(groupName, m_scheduler.GetJobNames(groupName));
            }

            foreach (string groupName in jobNames.Keys)
            {
                foreach (string jn in jobNames[groupName])
                {
                    m_scheduler.DeleteJob(jn, groupName);
                }
            }
        }

        private void Initialize()
        {
            if (null != m_schedulerListeners)
            {
                foreach (ISchedulerListener sl in m_schedulerListeners)
                {
                    m_scheduler.AddSchedulerListener(sl);
                }
            }

            if (null != m_jobListeners)
            {
                foreach (IJobListener jl in m_jobListeners)
                {
                    m_scheduler.AddGlobalJobListener(jl);
                }
            }

            if (m_clearAllJobs)
            {
                ClearAllJobs();

                if (0 < m_jobSchedulers.Count)
                {
                    foreach (IJobScheduler js in m_jobSchedulers)
                    {
                        if (js is IJobScheduler)
                        {
                            ((IJobScheduler)js).Schedule(m_scheduler, null);
                        }
                        else
                        {
                            js.Schedule(m_scheduler, null);
                        }
                    }
                }
            }

            if(m_scheduler.GetPausedTriggerGroups().Contains(AdoConstants.AllGroupsPaused))
            {
                Logger.INFO_Format(@"{0} PAUSED.  RESUMING.", AdoConstants.AllGroupsPaused);
                m_scheduler.ResumeAll();
                Logger.INFO_Format(@"{0} RESUMED.", AdoConstants.AllGroupsPaused);
            }
        }

        #region IContainerUser Members

        public virtual IIoCContainer Container { get; set; }

        #endregion

        #region ILogWriter Members

        public virtual ILogger Logger { get; set; }

        #endregion
    }
}