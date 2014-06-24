using System;
using Quartz;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Tunney.Common.Notifiers.Email;

namespace Tunney.Common.Scheduling.Jobs
{
    [Serializable]
    public abstract class AJob_SQLServer_StoredProcedureRunner : IJob
    {
        public const string JOBDETAILS_CONNECTIONSTRING = @"ConnectionString";
        public const string JOBDETAILS_STOREDPROC_FQN = @"StoredProcedure_FullyQualifiedName";
        public const string JOBDETAILS_STOREPROC_PARAMS_NAME_AND_VALUE_PAIRS = @"StoredProcParamValues";
        public const string JOBDETAILS_STOREDPROC_PARAMS_NAME_AND_TYPE_PAIRS = @"StoredProcParamTypes";
        public const string JOBDETAILS_EMAIL_NOTIFIER = @"EmailNotifier";

        protected AJob_SQLServer_StoredProcedureRunner()
        {
        }

        protected abstract void Execute(SqlCommand _preparedCommand, IEmailer _emailNotifier);

        #region IJob Members

        public void Execute(JobExecutionContext context)
        {
            IEmailer emailNotifier = (IEmailer)context.JobDetail.JobDataMap[JOBDETAILS_EMAIL_NOTIFIER];
            string connectionString = (string)context.JobDetail.JobDataMap[JOBDETAILS_CONNECTIONSTRING];
            string storedProc = (string)context.JobDetail.JobDataMap[JOBDETAILS_STOREDPROC_FQN];

            IDictionary<string, string> storedProcParams_NamesAndValues = (IDictionary<string, string>)context.JobDetail.JobDataMap[JOBDETAILS_STOREPROC_PARAMS_NAME_AND_VALUE_PAIRS];
            IDictionary<string, DbType> storedProcParams_NamesAndTypes = (IDictionary<string, DbType>)context.JobDetail.JobDataMap[JOBDETAILS_STOREDPROC_PARAMS_NAME_AND_TYPE_PAIRS];

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = storedProc;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Transaction = trans;

                                AddParameters(cmd, storedProcParams_NamesAndValues, storedProcParams_NamesAndTypes);

                                Execute(cmd, emailNotifier);
                            }
                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
                finally
                {
                    if (ConnectionState.Open == conn.State) conn.Close();
                }
            }
        }

        #endregion

        protected virtual void AddParameters(SqlCommand _cmd, IDictionary<string, string> _paramNamesAndValues, IDictionary<string, DbType> _paramNamesAndTypes)
        {
            //TODO:  Handle custom parameters here!
        }
    }
}