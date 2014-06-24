using System;
using System.Collections.Generic;

using Quartz;

using Tunney.Common.Scheduling.Jobs;

namespace Tunney.Common.Scheduling.JobSchedulers
{
    [Serializable]
    public class JobScheduler_InvokeExternalProcess : ACronJobScheduler
    {
        public const string SIMPLE_TRIGGER_CRON_SYNTAX = @"INVALID CRON SYNTAX";

        protected const string JOB_TYPE_FQCN = "Tunney.Common.Scheduling.Jobs.Job_InvokeExternalProcess, Tunney.Common";

        protected readonly string m_fullyQualifiedJobClassName;
        protected readonly string m_stagingDirectory;
        protected readonly string m_executableFilename;

        public JobScheduler_InvokeExternalProcess(string _cronSyntax, string _jobName, string _jobGroup, string _triggerName, string _jobConfiguratorIoCName, int _triggerPriority, string _fullyQualifiedJobClassName, string _stagingDirectory, string _executableFilename, string _jobIoCConfigurationFilename)
            : base(_cronSyntax, _jobName, _jobGroup, _triggerName, _jobConfiguratorIoCName, JOB_TYPE_FQCN, _triggerPriority, _jobIoCConfigurationFilename)
        {
            if (string.IsNullOrEmpty(_fullyQualifiedJobClassName)) throw new ArgumentNullException(@"_fullyQualifiedJobClassName");
            if (string.IsNullOrEmpty(_stagingDirectory)) throw new ArgumentNullException(@"_stagingDirectory");
            if (string.IsNullOrEmpty(_executableFilename)) throw new ArgumentNullException(@"_executableFilename");

            m_fullyQualifiedJobClassName = _fullyQualifiedJobClassName;
            m_stagingDirectory = _stagingDirectory;
            m_executableFilename = _executableFilename;
        }

        protected override void AddDataMapStuff(JobDetail _jobDetail)
        {
            _jobDetail.JobDataMap[Job_InvokeExternalProcess.CONFIG_EXE_FILENAME] = m_executableFilename;
            _jobDetail.JobDataMap[Job_InvokeExternalProcess.CONFIG_JOB_FQCN] = m_fullyQualifiedJobClassName;
            _jobDetail.JobDataMap[Job_InvokeExternalProcess.CONFIG_STAGING_DIRECTORY] = m_stagingDirectory;
        }

        public override Trigger MakeTrigger()
        {
            if (m_cronSyntax.Equals(SIMPLE_TRIGGER_CRON_SYNTAX))
            {
                SimpleTrigger t = new SimpleTrigger(m_triggerName, m_jobGroup, 0, TimeSpan.Zero);
                //t.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithExistingCount;
                t.MisfireInstruction = MisfireInstruction.SimpleTrigger.FireNow;
                return t;
            }
            else return base.MakeTrigger();
        }

        protected override Type GetJobType(string _fullyQualifiedClassName)
        {
            Type t = base.GetJobType(_fullyQualifiedClassName);

            Type baseType = typeof(Job_InvokeExternalProcess);

            if (!t.IsSubclassOf(baseType) && t != baseType)
            {
                throw new InvalidOperationException(string.Format(@"Defined job type is not {0} or a derived type.", GetType().FullName));
            }

            return t;
        }
    }
}