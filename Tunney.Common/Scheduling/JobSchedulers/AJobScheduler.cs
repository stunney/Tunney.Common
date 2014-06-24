using System;
using System.Collections.Generic;

using Quartz;
using Tunney.Common.Scheduling.Jobs;

namespace Tunney.Common.Scheduling.JobSchedulers
{
    [Serializable]
    public abstract class AJobScheduler : Tunney.Common.Scheduling.AJobScheduler
    {
        protected readonly string m_jobName;
        protected readonly string m_jobGroup;
        protected readonly string m_triggerName;

        protected readonly string m_jobConfiguratorIoCName;
        protected readonly string m_jobTypeFQCN;
        protected readonly int m_triggerPriority;
        protected readonly string m_iocConfigurationFilename;

        protected AJobScheduler(string _jobName, string _jobGroup, string _triggerName, string _jobConfiguratorIoCName, string _jobTypeFQCN, int _triggerPriority, string _iocConfigurationFilename)
        {
            if (string.IsNullOrEmpty(_jobName)) throw new ArgumentNullException(@"_jobName");
            if (string.IsNullOrEmpty(_jobGroup)) throw new ArgumentNullException(@"_jobGroup");
            if (string.IsNullOrEmpty(_triggerName)) throw new ArgumentNullException(@"_triggerName");
            if (string.IsNullOrEmpty(_jobConfiguratorIoCName)) throw new ArgumentNullException(@"_jobConfiguratorIoCName");
            if (string.IsNullOrEmpty(_jobTypeFQCN)) throw new ArgumentNullException(@"_jobTypeFQCN");
            if (string.IsNullOrEmpty(_iocConfigurationFilename)) throw new ArgumentNullException(@"_iocConfigurationFilename");

            if (0 >= _triggerPriority) throw new ArgumentOutOfRangeException(@"_triggerPriority", @"Trigger Priority must be greater than zero.");

            m_jobName = _jobName;
            m_jobGroup = _jobGroup;
            m_triggerName = _triggerName;

            m_jobConfiguratorIoCName = _jobConfiguratorIoCName;
            m_jobTypeFQCN = _jobTypeFQCN;
            m_triggerPriority = _triggerPriority;
            m_iocConfigurationFilename = _iocConfigurationFilename;
        }

        protected abstract void AddDataMapStuff(JobDetail _jobDetail);

        public override void Schedule(Quartz.IScheduler _scheduler, JobDataMap _extendedDataMap)
        {
            JobDetail jobDetail = _scheduler.GetJobDetail(m_jobName, m_jobGroup);
            Trigger trigger = _scheduler.GetTrigger(m_triggerName, m_jobGroup);            

            if (null != jobDetail) return;

            if (null != trigger)
            {
                _scheduler.UnscheduleJob(trigger.Name, trigger.Group);
            }

            jobDetail = new JobDetail(m_jobName, m_jobGroup, GetJobType(m_jobTypeFQCN));

            jobDetail.JobDataMap.PutAll(_extendedDataMap);

            jobDetail.JobDataMap[AJob.CONFIG_CONFIGURATOR_IOC_NAME] = m_jobConfiguratorIoCName;
            jobDetail.JobDataMap[AJob.CONFIG_IOC_CONFIG_FILENAME] = m_iocConfigurationFilename;

            AddDataMapStuff(jobDetail);

            trigger = MakeTrigger();
            trigger.Priority = m_triggerPriority;

            AJobScheduler.ScheduleJob(jobDetail, trigger, _scheduler);
        }

        public static void ScheduleJob(JobDetail _jobDetail, Trigger _trigger, IScheduler _scheduler)
        {
            IList<string> jobNames = new List<string>(_scheduler.GetJobNames(_jobDetail.Group));

            if (!jobNames.Contains(_jobDetail.Name))
                _scheduler.ScheduleJob(_jobDetail, _trigger);
        }

        public abstract Trigger MakeTrigger();

        public virtual string JobName { get { return m_jobName; } }
        public virtual string JobGroup { get { return m_jobGroup; } }
        public virtual string TriggerName { get { return m_triggerName; } }
        public virtual string JobConfiguratorIoCName { get { return m_jobConfiguratorIoCName; } }
        public virtual string JobTypeFQCN { get { return m_jobTypeFQCN; } }
        public virtual int TriggerPriority { get { return m_triggerPriority; } }
        public virtual string IoCConfigurationFilename { get { return m_iocConfigurationFilename; } }

        #region ISerializable Members

        private const string SERIALIZATION_JOB_NAME = @"JobName";
        private const string SERIALIZATION_JOB_GROUP = @"JobGroup";
        private const string SERIALIZATION_TRIGGER_NAME = @"TriggerName";
        private const string SERIALIZATION_JOB_TYPE_FQCN = @"JobTypeFQCN";
        private const string SERIALIZATION_JOB_CONFIG_IOC_NAME = @"JobConfiguratorIoCName";
        private const string SERIALIZATION_TRIGGER_PRIORITY = @"TriggerPriority";
        private const string SERIALIZATION_IOC_CONFIG_FILENAME = @"IOCConfigurationFilename";

        protected AJobScheduler(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            m_jobName = info.GetString(SERIALIZATION_JOB_NAME);
            m_jobGroup = info.GetString(SERIALIZATION_JOB_GROUP);
            m_triggerName = info.GetString(SERIALIZATION_TRIGGER_NAME);
            m_jobConfiguratorIoCName = info.GetString(SERIALIZATION_JOB_CONFIG_IOC_NAME);
            m_jobTypeFQCN = info.GetString(SERIALIZATION_JOB_TYPE_FQCN);
            m_triggerPriority = info.GetInt32(SERIALIZATION_TRIGGER_PRIORITY);
            m_iocConfigurationFilename = info.GetString(SERIALIZATION_IOC_CONFIG_FILENAME);
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(SERIALIZATION_JOB_NAME, m_jobName);
            info.AddValue(SERIALIZATION_JOB_GROUP, m_jobGroup);
            info.AddValue(SERIALIZATION_TRIGGER_NAME, m_triggerName);
            info.AddValue(SERIALIZATION_JOB_CONFIG_IOC_NAME, m_jobConfiguratorIoCName);
            info.AddValue(SERIALIZATION_JOB_TYPE_FQCN, m_jobTypeFQCN);
            info.AddValue(SERIALIZATION_TRIGGER_PRIORITY, m_triggerPriority);
            info.AddValue(SERIALIZATION_IOC_CONFIG_FILENAME, m_iocConfigurationFilename);
        }

        #endregion
    }
}