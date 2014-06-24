using System;
using System.Runtime.Serialization;

using Quartz;

namespace Tunney.Common.Scheduling.JobSchedulers
{
    public abstract class ACronJobScheduler : AJobScheduler
    {
        protected readonly string m_cronSyntax;

        /// <summary>
        /// Applies the given <paramref name="_cronSyntax"/> the the job's trigger.
        /// </summary>
        /// <param name="_iocContainer"></param>
        /// <param name="_dataHelperIoCName"></param>
        /// <param name="_cronSyntax"></param>
        protected ACronJobScheduler(string _cronSyntax, string _jobName, string _jobGroup, string _triggerName, string _jobConfiguratorIoCName, string _jobTypeFQCN, int _triggerPriority, string _jobIoCConfigurationFilename)
            : base(_jobName, _jobGroup, _triggerName, _jobConfiguratorIoCName, _jobTypeFQCN, _triggerPriority, _jobIoCConfigurationFilename)
        {            
            if (string.IsNullOrEmpty(_cronSyntax)) throw new ArgumentNullException(@"_cronSyntax");

            m_cronSyntax = _cronSyntax;

            MakeTrigger(); //Validate the cron syntax
        }

        public override Trigger MakeTrigger()
        {
            return ACronJobScheduler.MakeCronTrigger(m_triggerName, m_jobGroup, m_cronSyntax, m_triggerPriority);
        }

        internal static Trigger MakeCronTrigger(string _triggerName, string _jobGroup, string _cronSyntax, int _triggerPriority)
        {
            CronTrigger retval = new CronTrigger(_triggerName, _jobGroup, _cronSyntax);
            retval.StartTimeUtc = StartTimeUtc;
            retval.Priority = _triggerPriority;
            retval.MisfireInstruction = MisfireInstruction.CronTrigger.DoNothing;

            return retval;
        }

        #region Serializable Members
        
        private const string SER_CRON_SYNTAX = "CronSyntax";

        protected ACronJobScheduler(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            m_cronSyntax = info.GetString(SER_CRON_SYNTAX);

            MakeTrigger(); //Validate the cron syntax is okie dokie :)
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(SER_CRON_SYNTAX, m_cronSyntax);
        }

        #endregion
    }
}