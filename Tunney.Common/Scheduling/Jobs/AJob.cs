using System;
using System.Collections.Generic;
using System.Threading;

using Quartz;
using Quartz.Impl.AdoJobStore;

using Tunney.Common.IoC;
using Tunney.Common.Notifiers;
using Tunney.Common.Statistics;
using Tunney.Common.Notifiers.Email;
using System.IO;

namespace Tunney.Common.Scheduling.Jobs
{
    public abstract class AJob : IStatefulJob, ILogWriter, IContainerUser, IEmailSender, IStatisticsLogger
    {
        public const string CONFIG_CONFIGURATOR_IOC_NAME = @"ConfiguratorIoCName";

        public const string CONFIG_IOC_CONFIG_FILENAME = @"IOCConfigurationFilename";

        public const string CONFIG_PARENT_JOB_ID = @"ParentJobID";

        private IJobConfigurator m_jobConfigurator;

        public virtual string JobName { get; set; }
        public virtual string JobGroup { get; set; }

        public virtual IScheduler Scheduler { get; set; }

        public virtual string JobConfiguratorIoCName { get; set; }
        public virtual string JobIoCConfigurationFilename { get; set; }
        
        public virtual IJobConfigurator JobConfigurator
        {
            get
            {
                if (null == m_jobConfigurator)
                {
                    if (string.IsNullOrEmpty(JobConfiguratorIoCName)) throw new NullReferenceException(@"JobConfiguratorIoCName can not be null.  Can't get the JobConfigurator.");
                    m_jobConfigurator = Container.Resolve<IJobConfigurator>(JobConfiguratorIoCName);

                    /*******WARNING*******WARNING******WARNING*************************/
                    //NOTE:  Total hack!  Figure out how to inject the WindsorContainer properly using a facility or handler or whatever in Castle.Windsor.  Tried with the kernel, but it is insufficient.
                    m_jobConfigurator.Container = Container;
                    /*******WARNING*******WARNING******WARNING*************************/
                }
                return m_jobConfigurator;
            }
        }

        public abstract void ExecuteJob(JobDataMap dataMap);

        protected virtual void OnLastAttemptFailed(JobExecutionContext context, JobExecutionException _exception)
        {
            throw _exception;
        }

        #region IJob Members
        
        public virtual void Execute(JobExecutionContext context)            
        {
            Logger.DEBUG("AJob::Execute Called");

            try
            {
                JobName = context.JobDetail.Name;
                JobGroup = context.JobDetail.Group;
                Scheduler = context.Scheduler;

                ExecuteJob(context.JobDetail.JobDataMap);
            }
            catch (Exception ex)
            {
                JobExecutionException jobex = null;
                if (ex is JobExecutionException)
                {
                    jobex = (JobExecutionException)ex;
                }
                else
                {
                    jobex = new JobExecutionException(ex.Message, ex, true);
                }

                Logger.ERROR(jobex);

                //Thread.Sleep(5000);

                throw jobex;
            }
            finally
            {
                Logger.DEBUG("AJob::Execute Call Complete");
            }
        }

        #endregion

        protected virtual void TryToDeleteTempFiles(DirectoryInfo _directoryInfo)
        {
            try
            {
                _directoryInfo.Delete(true);
            }
            catch { }

            _directoryInfo.Refresh();
        }

        /// <summary>
        /// Splits a <see cref="List{T}"/> into multiple chunks.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to be chunked.</param>
        /// <param name="chunkSize">The size of each chunk.</param>
        /// <returns>A list of chunks.</returns>
        public static IList<IList<T>> SplitIntoChunks<T>(List<T> list, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("chunkSize must be greater than 0.");
            }

            List<IList<T>> retVal = new List<IList<T>>((int)(list.Count / chunkSize) + 1);
            int index = 0;
            while (index < list.Count)
            {
                int count = list.Count - index > chunkSize ? chunkSize : list.Count - index;
                retVal.Add(list.GetRange(index, count));

                index += chunkSize;
            }

            return retVal;
        }

        #region ILogWriter Members

        public virtual ILogger Logger { get; set; }

        #endregion

        #region IContainerUser Members

        public virtual IIoCContainer Container { get; set; }

        #endregion

        #region IEmailSender Members

        public IEmailer EmailNotifier{ get; set; }

        #endregion        
    
        #region IStatisticsLogger Members

        public IStatisticsDataAccess Stats { get; set; }

        #endregion
    }

    public sealed class DescendingShortSorter : IComparer<short>
    {
        public DescendingShortSorter()
        {
        }

        #region IComparer<short> Members

        public int Compare(short x, short y)
        {
            ////Descending order.
            if (x < y) return 1;
            if (y < x) return -1;

            return 0;
        }

        #endregion
    }

    public sealed class AscendingShortSorter : IComparer<short>
    {
        public AscendingShortSorter()
        {
        }

        #region IComparer<short> Members

        public int Compare(short x, short y)
        {
            //Ascending order.
            if (x < y) return -1;
            if (y < x) return 1;

            return 0;
        }

        #endregion
    }
}