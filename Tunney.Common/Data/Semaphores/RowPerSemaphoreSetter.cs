using System;
using System.Collections.Generic;
using System.Data;

namespace Tunney.Common.Data.Semaphores
{
    [Serializable]
    public class RowPerSemaphoreSetter : ASemaphoreSetter
    {
        protected readonly IDataHelper m_dataHelper;
        
        protected readonly string m_stampKey;

        public RowPerSemaphoreSetter(IDataHelper _dataHelper, string _semaphoreKey)
            : base(_semaphoreKey)
        {
            if (null == _dataHelper) throw new ArgumentNullException(@"_dataHelper");
            m_dataHelper = _dataHelper;
        }

        public RowPerSemaphoreSetter(IDataHelper _dataHelper, string _semaphoreKey, string _stampKey)
            : this(_dataHelper, _semaphoreKey)
        {
            if (string.IsNullOrEmpty(_stampKey)) throw new ArgumentNullException(@"_stampKey");            
            m_stampKey = _stampKey;
        }

        #region ISemaphoreSetter Members

        public override void Set(DateTimeOffset _value)
        {
            UpdateTaskTimeTrackerSempahore(m_semaphoreKey, _value);
            if (!string.IsNullOrEmpty(m_stampKey))
            {
                UpdateTaskTimeTrackerSempahore(m_stampKey, DateTime.Now);
            }
        }

        #endregion

        #region ISemaphoreChecker Members

        public override DateTimeOffset Check()
        {
            return GetLastTaskTrackerDetails()[m_semaphoreKey];
        }

        #endregion

        /// <summary>
        /// Gets the last endtime and time span for the task creation worker.
        /// </summary>
        /// <returns>
        /// 
        /// </returns>
        protected virtual IDictionary<string, DateTimeOffset> GetLastTaskTrackerDetails()
        {
            IDictionary<string, DateTimeOffset> retval = new Dictionary<string, DateTimeOffset>(10);

            try
            {
                using(IDbConnection conn = m_dataHelper.ControllerNodeConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"[dbo].[Automation_Get_Last_Time_Tracker_Sempahore_Values]";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(CreateDebugParam(cmd));

                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                string semName = dr.GetString(0);
                                DateTimeOffset stamp = (DateTimeOffset)dr[1];

                                if (!retval.ContainsKey(semName)) retval.Add(semName, stamp);
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

            return retval;
        }

        protected virtual void UpdateTaskTimeTrackerSempahore(string _semaphoreName, DateTimeOffset _stamp)
        {
            try
            {
                using(IDbConnection conn = m_dataHelper.ControllerNodeConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"[dbo].[Automation_Task_Time_Tracker_Semaphore_Update]";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(CreateParameter(cmd, "SemaphoreName", DbType.String, _semaphoreName));
                        cmd.Parameters.Add(CreateParameter(cmd, "Stamp", DbType.DateTimeOffset, _stamp));
                        cmd.Parameters.Add(CreateDebugParam(cmd));

                        ProcessCommand_RECURSIVE(cmd, 10);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(@"Unable to execute UpdateTaskTrackerDetails", ex);
            }            
        }
    }
}