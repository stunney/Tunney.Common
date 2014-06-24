using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Quartz;

using Tunney.Common.Scheduling.Jobs;

namespace Tunney.Common.Scheduling.JobSchedulers
{
    [Serializable]
    public class JobScheduler_GetExternalProcessCronJobsFromDatabase :  Tunney.Common.Scheduling.AJobScheduler
    {
        protected readonly string m_connectionString;
        protected readonly string m_query;

        public JobScheduler_GetExternalProcessCronJobsFromDatabase(string _connectionString, string _query)
        {
            if (string.IsNullOrEmpty(_connectionString)) throw new ArgumentNullException(@"_connectionString");
            if (string.IsNullOrEmpty(_query)) throw new ArgumentNullException(@"_query");

            m_connectionString = _connectionString;
            m_query = _query;
        }

        public virtual IList<CronJobScheduleInfo> GetCronJobsToSchedule()
        {
            List<CronJobScheduleInfo> retval = new List<CronJobScheduleInfo>(10);

            //NOTE:  We expect these columns in this order
            //[JobName], [JobGroup], [TriggerName], [TriggerPriority], [CronSyntax], [JobTypeFQCN], [ConfiguratorIoCName], [StagingDirectory], [ExecutableFilename], [IoCConfigFilename]
            //0,            1,          2,              3,              4,              5,              6,                  7,                  8,                      9

            using (SqlConnection conn = new SqlConnection(m_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = m_query;
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            CronJobScheduleInfo a = new CronJobScheduleInfo()
                            {
                                JobName = dr.GetString(0),
                                JobGroup = dr.GetString(1),
                                TriggerName = dr.GetString(2),
                                CronSyntax = dr.GetString(4),
                                TriggerPriority = dr.GetInt32(3),
                                JobTypeFQCN = dr.GetString(5),
                                StagingDirectory = dr.GetString(7),
                                ExecutableFilename = dr.GetString(8),
                                ConfiguratorIoCName = dr.GetString(6),
                                IoCConfigFilename = dr.GetString(9)
                            };

                            retval.Add(a);
                        }
                    }
                }
            }

            return retval.AsReadOnly();
        }

        public override void Schedule(Quartz.IScheduler _scheduler, JobDataMap _extendedDataMap)
        {
            foreach (CronJobScheduleInfo a in GetCronJobsToSchedule())
            {
                ScheduleJob(_scheduler, a);
            }               
        }        

        public virtual void ScheduleJob(Quartz.IScheduler _scheduler, CronJobScheduleInfo _scheduleInfo)
        {
            JobDetail jobDetail = _scheduler.GetJobDetail(_scheduleInfo.JobName, _scheduleInfo.JobGroup);
            Trigger trigger = _scheduler.GetTrigger(_scheduleInfo.TriggerName, _scheduleInfo.JobGroup);

            if (null != jobDetail) return;

            if (null != trigger)
            {
                _scheduler.UnscheduleJob(trigger.Name, trigger.Group);
            }

            jobDetail = new JobDetail(_scheduleInfo.JobName, _scheduleInfo.JobGroup, typeof(Tunney.Common.Scheduling.Jobs.Job_InvokeExternalProcess));

            jobDetail.JobDataMap[AJob.CONFIG_CONFIGURATOR_IOC_NAME] = _scheduleInfo.ConfiguratorIoCName;
            jobDetail.JobDataMap[Job_InvokeExternalProcess.CONFIG_EXE_FILENAME] = _scheduleInfo.ExecutableFilename;
            jobDetail.JobDataMap[Job_InvokeExternalProcess.CONFIG_STAGING_DIRECTORY] = _scheduleInfo.StagingDirectory;
            jobDetail.JobDataMap[Job_InvokeExternalProcess.CONFIG_JOB_FQCN] = _scheduleInfo.JobTypeFQCN;
            jobDetail.JobDataMap[Job_InvokeExternalProcess.CONFIG_IOC_CONFIG_FILENAME] = _scheduleInfo.IoCConfigFilename;

            trigger = ACronJobScheduler.MakeCronTrigger(_scheduleInfo.TriggerName, _scheduleInfo.JobGroup, _scheduleInfo.CronSyntax, _scheduleInfo.TriggerPriority);
            trigger.MisfireInstruction = MisfireInstruction.CronTrigger.FireOnceNow;

            AJobScheduler.ScheduleJob(jobDetail, trigger, _scheduler);
        }

        public virtual void UnscheduleJob(Quartz.IScheduler _scheduler, CronJobScheduleInfo _scheduleInfo)
        {
            JobDetail jobDetail = _scheduler.GetJobDetail(_scheduleInfo.JobName, _scheduleInfo.JobGroup);
            Trigger trigger = _scheduler.GetTrigger(_scheduleInfo.TriggerName, _scheduleInfo.JobGroup);

            if (null != trigger)
            {
                _scheduler.UnscheduleJob(trigger.Name, trigger.Group);
            }
        }

        public class CronJobScheduleInfo
        {
            public virtual string JobName { get; set; }
            public virtual string JobGroup { get; set; }
            public virtual string TriggerName { get; set; }
            public virtual int TriggerPriority { get; set; }
            public virtual string CronSyntax { get; set; }
            public virtual string JobTypeFQCN { get; set; }
            public virtual string ConfiguratorIoCName { get; set; }
            public virtual string StagingDirectory { get; set; }
            public virtual string ExecutableFilename { get; set; }
            public virtual string IoCConfigFilename { get; set; }

            public override string ToString()
            {
                return JobName;
            }
        }
    }
}