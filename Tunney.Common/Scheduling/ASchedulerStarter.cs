using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Quartz;
using Quartz.Impl;
using System.Threading;
using Quartz.Impl.AdoJobStore;
using Quartz.Simpl;
using Tunney.Common.IoC;
using Tunney.Common.Notifiers.Email;


namespace Tunney.Common.Scheduling
{
    [Serializable]
    public abstract class ASchedulerStarter : IScheduleStarter
    {
        private readonly IList<IJobScheduler> m_jobSchedulers;
        private readonly IList<IJobListener> m_jobListeners;
        private readonly IList<ISchedulerListener> m_schedulerListeners;        
        private readonly IEmailer m_emailer;
        private readonly IScheduler m_scheduler;

        protected ASchedulerStarter(ISchedulerFactory _schedulerFactory, IList<IJobScheduler> _jobSchedulers, IList<IJobListener> _jobListeners, IList<ISchedulerListener> _schedulerListeners, IEmailer _emailer)
        {
            if (null == _schedulerFactory) throw new ArgumentNullException(@"_schedulerFactory");
            if (null == _jobSchedulers) throw new ArgumentNullException(@"_jobSchedulers");
            if (null == _emailer) throw new ArgumentNullException(@"_emailer");

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
            m_jobSchedulers = _jobSchedulers;
            m_emailer = _emailer;
        }       

        public virtual IEmailer Emailer
        {
            get { return m_emailer; }
        }

        public virtual void Start()
        {
            try
            {
                Initialize();

                m_scheduler.Start();
            }
            catch (Exception _ex)
            {
                Logger.FATAL(_ex);
                Emailer.Send(@"Sentry 2.0 Error", @"Check the logs, an error occured while starting the service up");
            }
        }

        public virtual void Stop()
        {
            try
            {
                m_scheduler.Shutdown();
            }
            catch (Exception _ex)
            {
                Logger.FATAL(_ex);
                Emailer.Send(@"Sentry 2.0 Error", @"Check the logs, an error occured while shutting the service down");
            }
        }

        public virtual void Continue()
        {
            try
            {
                m_scheduler.ResumeAll();
            }
            catch (Exception _ex)
            {
                Logger.FATAL(_ex);
                Emailer.Send(@"Sentry 2.0 Error", @"Check the logs, an error occured while continuing the service");
            }
        }

        public virtual void Pause()
        {
            try
            {
                m_scheduler.PauseAll();
            }
            catch (Exception _ex)
            {
                Logger.FATAL(_ex);
                Emailer.Send(@"Sentry 2.0 Error", @"Check the logs, an error occured while pausing the service");
            }
        }

        protected virtual void ClearAllJobs()
        {
            IList<string> jobNames = m_scheduler.GetJobNames(null);

            foreach (string jn in jobNames)
                m_scheduler.DeleteJob(jn, null);
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


            if (0 < m_jobSchedulers.Count)
            {                
                foreach (IJobScheduler js in m_jobSchedulers)
                {
                    js.Schedule(m_scheduler, null); //5 minutes
                }
            }
            
            if(m_scheduler.GetPausedTriggerGroups().Contains(AdoConstants.AllGroupsPaused))
            {
                Logger.INFO_Format(@"{0} PAUSED.  RESUMING.", AdoConstants.AllGroupsPaused);
                m_scheduler.ResumeAll();
                Logger.INFO_Format(@"{0} RESUMED.", AdoConstants.AllGroupsPaused);
            }
        }

        #region ILogWriter Members

        public virtual ILogger Logger { get; set; }

        #endregion

        #region IContainerUser Members

        public virtual IIoCContainer Container { get; set; }

        #endregion
    }
}