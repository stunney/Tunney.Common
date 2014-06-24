using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Tunney.Common.Data.Semaphores
{
    [Serializable]
    public class RowPerSemaphoreCreator : RowPerSemaphoreSetter, ISemaphoreCreator
    {
        private const string DATETIME_START_OF_CURRENT_DAY_FORMAT = "{0}/{1}/{2} 00:00:00";

        public RowPerSemaphoreCreator(IDataHelper _dataHelper, string _semaphoreKey)
            : base(_dataHelper, _semaphoreKey)
        {
        }

        #region ISemaphoreCreator Members

        public virtual void Create(DateTimeOffset _originalStampValue)
        {
            CreateSemaphore(_originalStampValue);
        }

        public virtual void Create()
        {
            DateTimeOffset startOfThisMonth = DateTimeOffset.Parse(string.Format(DATETIME_START_OF_CURRENT_DAY_FORMAT, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));

            CreateSemaphore(startOfThisMonth);
        }

        #endregion

        protected virtual void CreateSemaphore(DateTimeOffset _originalValue)
        {
            IDictionary<string, DateTimeOffset> retval = new Dictionary<string, DateTimeOffset>(10);

            try
            {
                using(IDbConnection conn = m_dataHelper.ControllerNodeConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"[dbo].[Automation_Create_Time_Tracker_Sempahore]";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(CreateParameter(cmd, "SemaphoreName", DbType.String, m_semaphoreKey));
                        cmd.Parameters.Add(CreateParameter(cmd, "SemaphoreValue", DbType.DateTime, _originalValue));
                        cmd.Parameters.Add(CreateDebugParam(cmd));

                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(@"Unable to execute stored procedure called [Automation_Create_Time_Tracker_Sempahore].", ex);
            }            
        }
    }
}