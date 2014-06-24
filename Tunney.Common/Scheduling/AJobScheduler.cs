using System;
using Quartz;
using System.Collections.Generic;

namespace Tunney.Common.Scheduling
{
    [Serializable]
    public abstract class AJobScheduler : IJobScheduler
    {
        protected TimeSpan m_jobFrequency = TimeSpan.FromSeconds(30.0d);  //Default to something simple.  30 seconds should do.

        protected AJobScheduler()
        {
        }

        #region IJobScheduler Members

        public abstract void Schedule(Quartz.IScheduler _scheduler, JobDataMap _extendedDataMap);

        public virtual TimeSpan JobFrequency
        {
            get
            {
                return m_jobFrequency;
            }
            set
            {
                if (TimeSpan.Zero >= value)
                {
                    throw new ArgumentOutOfRangeException("value", "Value has to be greater than zero.");
                }
                m_jobFrequency = value;
            }
        }

        #endregion

        protected virtual void ClearJobLike(IScheduler _scheduler, string _jobNameStart, string _jobGroup)
        {
            IList<string> jobNames = _scheduler.GetJobNames(_jobGroup);

            foreach (string jn in jobNames)
            {
                if (jn.StartsWith(_jobNameStart)) _scheduler.DeleteJob(jn, _jobGroup);
            }
        }

        protected virtual Type GetJobType(string _fullyQualifiedClassName)
        {
            Type t = Type.GetType(_fullyQualifiedClassName, true, false);

            if (t.GetInterface("Quartz.IJob", false) == null)
            {
                throw new ArgumentException(string.Format("The job with class name '{0}' does not inherit from Quartz.IJob.", _fullyQualifiedClassName), "_fullyQualifiedClassName");
            }

            return t;
        }

        protected static DateTime StartTimeUtc
        {
            get { return TriggerUtils.GetEvenMinuteDate(DateTime.UtcNow); }
        }

        #region ISerializable Members

        private const string SERIALIZATION_JOB_FREQUENCY = @"JobFrequency";

        protected AJobScheduler(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            m_jobFrequency = TimeSpan.FromSeconds(info.GetDouble(SERIALIZATION_JOB_FREQUENCY));
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue(SERIALIZATION_JOB_FREQUENCY, m_jobFrequency.TotalSeconds);
        }

        #endregion
    }
}