using System;
using Quartz;

using System.Text;
using System.Collections;
using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling
{
    [Serializable]
    public abstract class ALoggingJobListener : IJobListener
    {
        private readonly ILogger m_logger;

        protected ALoggingJobListener(ILogger _logger)
        {
            if (null == _logger)
            {
                throw new ArgumentNullException(@"_logger");
            }
            m_logger = _logger;
        }

        public virtual ILogger Logger
        {
            get { return m_logger; }
        }

        #region IJobListener Members

        public abstract void JobExecutionVetoed(JobExecutionContext context);

        public abstract void JobToBeExecuted(JobExecutionContext context);

        public abstract void JobWasExecuted(JobExecutionContext context, JobExecutionException jobException);

        public abstract string Name { get; }

        #endregion

        protected virtual string BuildJobDetails(JobDetail _jobDetail)
        {
            StringBuilder sb = new StringBuilder(1000);

            sb.AppendFormat("Job Name:{0}\r\n", _jobDetail.Name);
            sb.AppendFormat("Job Group:{0}\r\n", _jobDetail.Group);

            foreach (object key in _jobDetail.JobDataMap.Keys)
            {
                if (key is string)
                {
                    string strKey = (string)key;

                    if (_jobDetail.JobDataMap[key] is IList)
                    {
                        sb.AppendFormat("JDM\tKey:{0} Count='{1}'\r\n", strKey, ((IList)_jobDetail.JobDataMap[key]).Count);
                    }
                    else
                    {
                        object val = _jobDetail.JobDataMap[key];
                        string toWrite = string.Empty;
                        if (null == val)
                        {
                            toWrite = "NULL";
                        }
                        else
                        {
                            if (val is string) toWrite = (string)val;
                            else toWrite = val.ToString();
                        }

                        sb.AppendFormat("JDM\tKey:{0}='{1}'\r\n", strKey, toWrite);
                    }
                }
            }

            return sb.ToString();
        }
    }
}