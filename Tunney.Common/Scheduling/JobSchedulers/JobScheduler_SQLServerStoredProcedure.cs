using System;
using System.Collections.Generic;
using System.Data;

using Quartz;

using Tunney.Common.Notifiers.Email;
using Tunney.Common.Scheduling.Jobs;

namespace Tunney.Common.Scheduling.JobSchedulers
{
    [Serializable]
    public class JobScheduler_SQLServerStoredProcedure : Tunney.Common.Scheduling.AJobScheduler
    {
        public const string JOB_NAME_START = @"JobScheduler_SQLServerStoredProcedure_Name_{0}";
        public const string JOB_NAME_FORMAT = JOB_NAME_START + @"_{1}";
        public const string TRIGGER_NAME_FORMAT = @"JobScheduler_SQLServerStoredProcedure_Trigger_{0}_{1}";

        private readonly IEmailer m_emailNotifier;
        private readonly string m_sqlServerConnectionString;
        private readonly string m_fullyQualifiedStoredProcedureName;
        private readonly IDictionary<string, string> m_nameValueParameters;
        private readonly IDictionary<string, DbType> m_paramTypes;
        private readonly string m_fullyQualifiedJobClassName;
        private readonly bool m_clearScheduledJobOnStartup = false;

        public JobScheduler_SQLServerStoredProcedure(IEmailer _emailNotifier,
                                                    string _sqlServerConnectionString,
                                                    string _fullyQualifiedStoredProcedureName,
                                                    IDictionary<string, string> _nameValueParameters,
                                                    IDictionary<string, DbType> _paramTypes,                                                    
                                                    string _fullyQualifiedJobClassName,
                                                    bool _clearJobOnStartup)
            : base()
        {
            if (null == _emailNotifier)
            {
                throw new ArgumentNullException(@"_emailNotifier");
            }

            if (string.IsNullOrEmpty(_sqlServerConnectionString))
            {
                throw new ArgumentNullException(@"_sqlServerConnectionString");
            }

            if (string.IsNullOrEmpty(_fullyQualifiedStoredProcedureName))
            {
                throw new ArgumentNullException(@"_fullyQualifiedStoredProcedureName");
            }

            if (null == _nameValueParameters)
            {
                throw new ArgumentNullException(@"_nameValueParameters");
            }

            if (null == _paramTypes)
            {
                throw new ArgumentNullException(@"_paramTypes");
            }

            if (string.IsNullOrEmpty(_fullyQualifiedJobClassName))
            {
                throw new ArgumentNullException(@"_fullyQualifiedJobClassName");
            }

            m_emailNotifier = _emailNotifier;
            m_sqlServerConnectionString = _sqlServerConnectionString;
            m_fullyQualifiedStoredProcedureName = _fullyQualifiedStoredProcedureName;
            m_nameValueParameters = _nameValueParameters;
            m_paramTypes = _paramTypes;
            m_fullyQualifiedJobClassName = _fullyQualifiedJobClassName;
            m_clearScheduledJobOnStartup = _clearJobOnStartup;

            //Just the tiniest bit of validation here so that we get the error on scheduler startup rather than during the course of the scheduler.
            try
            {
                GetJobType(_fullyQualifiedJobClassName);
            }
            catch (Exception _ex)
            {
                throw new ArgumentException("There was an error initializing the job scheduler.  Check the inner exception for more details", @"_fullyQualifiedJobClassName", _ex);
            }
        }

        protected override Type GetJobType(string _fullyQualifiedClassName)
        {
            Type t = base.GetJobType(_fullyQualifiedClassName);
 
            if (!t.IsSubclassOf(typeof(AJob_SQLServer_StoredProcedureRunner)))
            {
                throw new ArgumentException(string.Format("The job with class name '{0}' does not inherit from Tunney.Scheduler.Utilities.Jobs.AJob_SQLServer_StoredProcedureRunner.", m_fullyQualifiedJobClassName), "_fullyQualifiedClassName");
            }

            return t;
        }

        #region IJobScheduler Members

        public override void Schedule(Quartz.IScheduler _scheduler, JobDataMap _extendedDataMap)
        {
            if (m_clearScheduledJobOnStartup)
            {
                ClearJobLike(_scheduler, string.Format(JOB_NAME_START, m_fullyQualifiedStoredProcedureName), null);
            }

            string jobName = string.Format(JOB_NAME_FORMAT, m_fullyQualifiedStoredProcedureName, Guid.NewGuid());
            string triggerName = string.Format(TRIGGER_NAME_FORMAT, m_fullyQualifiedStoredProcedureName, Guid.NewGuid());

            JobDetail jobDetail = _scheduler.GetJobDetail(jobName, null);
            Trigger trigger = _scheduler.GetTrigger(triggerName, null);

            if (null == jobDetail)
            {
                // construct job info
                jobDetail = new JobDetail(jobName, null, GetJobType(m_fullyQualifiedJobClassName));

                jobDetail.JobDataMap.PutAll(_extendedDataMap);

                jobDetail.JobDataMap[AJob_SQLServer_StoredProcedureRunner.JOBDETAILS_CONNECTIONSTRING] = m_sqlServerConnectionString;
                jobDetail.JobDataMap[AJob_SQLServer_StoredProcedureRunner.JOBDETAILS_STOREDPROC_FQN] = m_fullyQualifiedStoredProcedureName;
                jobDetail.JobDataMap[AJob_SQLServer_StoredProcedureRunner.JOBDETAILS_STOREPROC_PARAMS_NAME_AND_VALUE_PAIRS] = m_nameValueParameters;
                jobDetail.JobDataMap[AJob_SQLServer_StoredProcedureRunner.JOBDETAILS_STOREDPROC_PARAMS_NAME_AND_TYPE_PAIRS] = m_paramTypes;
                jobDetail.JobDataMap[AJob_SQLServer_StoredProcedureRunner.JOBDETAILS_EMAIL_NOTIFIER] = m_emailNotifier;
            }

            if (null == trigger)
            {
                trigger = TriggerUtils.MakeSecondlyTrigger(triggerName, (int)JobFrequency.TotalSeconds, -1);
                
                trigger.StartTimeUtc = TriggerUtils.GetEvenMinuteDate(DateTime.UtcNow);
                trigger.Priority = 100;
                trigger.MisfireInstruction = MisfireInstruction.SmartPolicy;
            }

            IList<string> jobNames = new List<string>(_scheduler.GetJobNames(null));

            if (!jobNames.Contains(jobName))
                _scheduler.ScheduleJob(jobDetail, trigger);
        }       

        #endregion

        #region ISerializable Members

        private const string SERIALIZATION_CONNECTION_STRING = "ConnectionString";
        private const string SERIALIZATION_STORED_PROC_NAME = "StoredProcName";
        private const string SERIALIZATION_NAME_VALUE_PARAMS = "NameValueParams";
        private const string SERIALIZATION_NAME_TYPE_PARAMS = "ParamNameToDbType";
        private const string SERIALIZATION_FQN_JOB_CLASS_NAME = @"JobClassName";
        private const string SERIALIZATION_CLEAR_JOB_FIRST = @"ClearJobFirst";
        private const string SERIALIZATION_EMAIL_NOTIFIER = @"Notifier";

        protected JobScheduler_SQLServerStoredProcedure(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            m_sqlServerConnectionString = info.GetString(SERIALIZATION_CONNECTION_STRING);
            m_fullyQualifiedStoredProcedureName = info.GetString(SERIALIZATION_STORED_PROC_NAME);
            m_nameValueParameters = (IDictionary<string, string>)info.GetValue(SERIALIZATION_NAME_VALUE_PARAMS, typeof(IDictionary<string, string>));
            m_paramTypes = (IDictionary<string, DbType>)info.GetValue(SERIALIZATION_NAME_TYPE_PARAMS, typeof(IDictionary<string, DbType>));
            m_fullyQualifiedJobClassName = info.GetString(SERIALIZATION_FQN_JOB_CLASS_NAME);
            m_clearScheduledJobOnStartup = info.GetBoolean(SERIALIZATION_CLEAR_JOB_FIRST);
            m_emailNotifier = (IEmailer)info.GetValue(SERIALIZATION_EMAIL_NOTIFIER, typeof(IEmailer));
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(SERIALIZATION_FQN_JOB_CLASS_NAME, m_fullyQualifiedJobClassName);
            info.AddValue(SERIALIZATION_CONNECTION_STRING, m_sqlServerConnectionString);
            info.AddValue(SERIALIZATION_STORED_PROC_NAME, m_fullyQualifiedStoredProcedureName);
            info.AddValue(SERIALIZATION_NAME_VALUE_PARAMS, m_nameValueParameters);
            info.AddValue(SERIALIZATION_NAME_TYPE_PARAMS, m_paramTypes);
            info.AddValue(SERIALIZATION_CLEAR_JOB_FIRST, m_clearScheduledJobOnStartup);
            info.AddValue(SERIALIZATION_EMAIL_NOTIFIER, m_emailNotifier);
        }

        #endregion
    }
}