using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Quartz;

using Tunney.Common.Data;
using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling.Listeners
{
    [Serializable]
    public class JobListener_ExecutionHistoryToSQLServer : DataHelperConstants, IJobListener, ILogWriter
    {
        protected readonly string m_connectionString;
        protected readonly string m_tableName;

        protected readonly IDbConnection m_connection;

        protected readonly string m_machineName = Environment.MachineName;

        public JobListener_ExecutionHistoryToSQLServer(string _connectionString, string _tableName)
        {
            if (string.IsNullOrEmpty(_connectionString)) throw new ArgumentNullException(@"_connectionString");
            if (string.IsNullOrEmpty(_tableName)) throw new ArgumentNullException(@"_tableName");            

            m_connectionString = _connectionString;
            m_tableName = _tableName;

            m_connection = new SqlConnection(m_connectionString);

            m_connection.Open();
        }

        public virtual ILogger Logger { get; set; }

        #region IJobListener Members

        private const string SQL_INSERT_FORMAT = "INSERT INTO [{0}] ([Stamp], [Machine], [JobName], [JobGroup]) VALUES (@Stamp, @Machine, @JobName, @JobGroup);";

        private const string SQL_POOL_EX_MSG = @"The timeout period elapsed prior to obtaining a connection from the pool.  This may have occurred because all pooled connections were in use and max pool size was reached.";

        public virtual void JobToBeExecuted(JobExecutionContext context)
        {
            try
            {
                using (IDbCommand cmd = m_connection.CreateCommand())
                {
                    cmd.CommandText = string.Format(SQL_INSERT_FORMAT, m_tableName);
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.Add(CreateParameter(cmd, "Stamp", SqlDbType.DateTime, context.FireTimeUtc.Value.ToLocalTime()));
                    cmd.Parameters.Add(CreateParameter(cmd, "Machine", SqlDbType.VarChar, m_machineName + "::" + context.Scheduler.SchedulerInstanceId));
                    cmd.Parameters.Add(CreateParameter(cmd, "JobName", SqlDbType.VarChar,  context.JobDetail.Name));
                    cmd.Parameters.Add(CreateParameter(cmd, "JobGroup", SqlDbType.VarChar, context.JobDetail.Group));

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (ConnectionState.Open != m_connection.State)
                {
                    m_connection.Close();
                    m_connection.Open();
                }
                Logger.ERROR(ex);
                if (ex is SqlException && ex.Message.Contains(SQL_POOL_EX_MSG))
                {
                    Logger.INFO(@"Clearing connection pool");
                    SqlConnection.ClearPool((SqlConnection)m_connection);
                }                
            }
        }

        public virtual void JobExecutionVetoed(JobExecutionContext context)
        {
        }

        public virtual void JobWasExecuted(JobExecutionContext context, JobExecutionException jobException)
        {            
        }

        public virtual string Name
        {
            get { return GetType().Name; }
        }

        #endregion
    }
}