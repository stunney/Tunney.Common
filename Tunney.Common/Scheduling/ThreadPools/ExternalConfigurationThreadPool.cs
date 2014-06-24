using System;
using System.Data;
using System.Data.SqlClient;
using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling.ThreadPools
{
    public abstract class ExternalConfigurationThreadPool : Quartz.Simpl.SimpleThreadPool
    {
        protected readonly IThreadPoolConfigurator m_threadPoolConfigurator;

        protected ExternalConfigurationThreadPool(IThreadPoolConfigurator _threadPoolConfigurator)
            : base(_threadPoolConfigurator.ThreadCount, System.Threading.ThreadPriority.Normal)
        {
            m_threadPoolConfigurator = _threadPoolConfigurator;
        }

        protected override System.Collections.IList CreateWorkerThreads(int threadCount)
        {
            return base.CreateWorkerThreads(m_threadPoolConfigurator.ThreadCount);
        }

        public new int ThreadCount
        {
            get { return m_threadPoolConfigurator.ThreadCount; }
            set { }
        }
    }

    public class CastleWindsorConfigurationThreadPool : ExternalConfigurationThreadPool
    {
        public const string THREADPOOL_CONFIGURATOR_FIXED_IOC_NAME = @"ThreadPool.Configurator";

        public CastleWindsorConfigurationThreadPool()
            : base(new CastleWindsorContainer(new Log4NetLogger()).Resolve<IThreadPoolConfigurator>(THREADPOOL_CONFIGURATOR_FIXED_IOC_NAME))
        {
        }
    }

    public interface IThreadPoolConfigurator
    {
        int ThreadCount { get; }
    }

    [Serializable]
    public class SQLServerViewThreadPoolConfigurator : IThreadPoolConfigurator, ILogWriter
    {
        protected readonly string m_connectionString;
        protected readonly string m_sqlCommand;

        protected int m_cachedThreadPoolCount = -1;

        public SQLServerViewThreadPoolConfigurator(string _connectionString, string _sqlCommand)
        {
            if (string.IsNullOrEmpty(_connectionString)) throw new ArgumentNullException(@"_connectionString");
            if (string.IsNullOrEmpty(_sqlCommand)) throw new ArgumentNullException(@"_sqlCommand");

            //NOTE:  Expects something like "SELECT TOP 1 [Affinity] FROM [Table/ViewX] WHERE [MachineName] LIKE @machineName + '.%';"
            if (!_sqlCommand.Contains(@"@machineName")) throw new ArgumentException(@"Should contain '@machineName' parameter to get proper affinity", @"_sqlCommand");

            m_connectionString = _connectionString;
            m_sqlCommand = _sqlCommand;
        }

        public virtual int ThreadCount
        {
            get
            {
                if (-1 == m_cachedThreadPoolCount)
                {
                    int newCachedValue = GetThreadCountFromDatabase();
                    if (-1 != newCachedValue) m_cachedThreadPoolCount = newCachedValue;
                }

                return m_cachedThreadPoolCount;
            }
        }

        protected virtual int GetThreadCountFromDatabase()
        {
            int retval = 1;
            try
            {
                using (IDbConnection conn = new SqlConnection(m_connectionString))
                {
                    conn.Open();

                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = m_sqlCommand;
                        cmd.CommandType = CommandType.Text;

                        IDbDataParameter machineNameParam = cmd.CreateParameter();
                        machineNameParam.ParameterName = @"machineName";
                        machineNameParam.DbType = DbType.String;
                        machineNameParam.Value = Environment.MachineName;

                        cmd.Parameters.Add(machineNameParam);

                        retval = (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception _ex)
            {
                ApplicationException appEx = new ApplicationException(@"Error configuring quartz ThreadPool using ExternalConfigurationThreadPool", _ex);
                Logger.WARN(appEx);

                retval = 1;
            }
            return retval;
        }

        public virtual ILogger Logger { get; set; }
    }
}