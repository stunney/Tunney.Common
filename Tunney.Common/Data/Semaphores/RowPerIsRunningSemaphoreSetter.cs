using System;
using System.Collections.Generic;
using System.Data;

namespace Tunney.Common.Data.Semaphores
{
    public class RowPerIsRunningSemaphoreSetter : DataHelperConstants, IRunningSemaphoreChecker, IRunningSemaphoreSetter
    {
        //Automations_Task_Running_Semaphore_Update
        //Automation_Get_Running_Semaphore_Values

        protected readonly string m_semaphoreName;
        protected readonly IDataHelper m_dataHelper;

        public RowPerIsRunningSemaphoreSetter(string _semaphoreName, IDataHelper _dataHelper)
        {
            if (string.IsNullOrEmpty(_semaphoreName)) throw new ArgumentNullException("_semaphoreName");
            if (null == _dataHelper) throw new ArgumentNullException("_dataHelper");

            m_semaphoreName = _semaphoreName;
            m_dataHelper = _dataHelper;
        }

        public virtual bool Check()
        {
            return GetRunningTasks().Contains(m_semaphoreName);
        }

        public virtual void Set(bool _isRunning)
        {
            UpdateTaskTimeTrackerSempahore(m_semaphoreName, _isRunning);
        }

        protected virtual IList<string> GetRunningTasks()
        {
            List<string> retval = new List<string>(10);

            try
            {
                using (IDbConnection conn = m_dataHelper.ControllerNodeConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"[dbo].[Automation_Get_Running_Semaphore_Values]";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(CreateDebugParam(cmd));

                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                string semName = dr.GetString(0);
                                if (!retval.Contains(semName)) retval.Add(semName);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(@"Unable to execute GetLastTaskTrackerDetails.", ex);
            }

            return retval.AsReadOnly();
        }

        protected virtual void UpdateTaskTimeTrackerSempahore(string _semaphoreName, bool _isRunning)
        {
            try
            {
                using (IDbConnection conn = m_dataHelper.ControllerNodeConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"[dbo].[Automation_Task_Running_Semaphore_Update]";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(CreateParameter(cmd, "semaphoreName", DbType.String, _semaphoreName));
                        cmd.Parameters.Add(CreateParameter(cmd, "isRunning", DbType.Boolean, _isRunning));
                        cmd.Parameters.Add(CreateDebugParam(cmd));

                        ProcessCommand_RECURSIVE(cmd, 10);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(@"Unable to execute UpdateTaskIsRunningDetails", ex);
            }
        }
    }
}
