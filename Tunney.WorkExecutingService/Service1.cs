using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using Tunney.Common.Scheduling;

namespace Tunney.WorkExecutingService
{
    public partial class Service1 : ServiceBase
    {
        private readonly IScheduleStarter m_scheduleStarter;

        public Service1(IScheduleStarter _scheduleStarter)
        {
            InitializeComponent();

            if (null == _scheduleStarter)
            {
                throw new ArgumentNullException(@"_scheduleStarter");
            }

            m_scheduleStarter = _scheduleStarter;

            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
        }

        protected override void OnStart(string[] args)
        {
            m_scheduleStarter.Start();
        }

        protected override void OnStop()
        {
            m_scheduleStarter.Stop();
        }

        protected override void OnContinue()
        {
            base.OnContinue();

            m_scheduleStarter.Continue();
        }

        protected override void OnPause()
        {
            base.OnPause();

            m_scheduleStarter.Pause();
        }
    }
}