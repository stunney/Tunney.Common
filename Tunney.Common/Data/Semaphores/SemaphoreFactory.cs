using System;
using System.Collections.Generic;

namespace Tunney.Common.Data.Semaphores
{
    [Serializable]
    public class SemaphoreFactory : ISemaphoreFactory
    {
        protected readonly IList<string> m_rowPerSemaphoreNames;
        protected readonly IList<string> m_singleTableRowSemaphoreNames;
        protected readonly string m_singleRowTableSemaphoreConnectionString;
        protected readonly string m_singleRowTableSemaphoreTableName;

        public SemaphoreFactory(IList<string> _rowPerSemaphoreNames, IList<string> _singleTableRowSemaphoreNames, string _singleRowTableSemaphoreConnectionString, string _singleRowTableSemaphoreTableName)
        {
            if (null == _rowPerSemaphoreNames) throw new ArgumentNullException(@"_rowPerSemaphoreNames");
            if (null == _singleTableRowSemaphoreNames) throw new ArgumentNullException(@"_singleTableRowSemaphoreNames");
            if (string.IsNullOrEmpty(_singleRowTableSemaphoreConnectionString)) throw new ArgumentNullException(@"_singleRowTableSemaphoreConnectionString");
            if (string.IsNullOrEmpty(_singleRowTableSemaphoreTableName)) throw new ArgumentNullException(@"_singleRowTableSemaphoreTableName");
            
            m_rowPerSemaphoreNames = _rowPerSemaphoreNames;
            m_singleTableRowSemaphoreNames = _singleTableRowSemaphoreNames;
            m_singleRowTableSemaphoreConnectionString = _singleRowTableSemaphoreConnectionString;
            m_singleRowTableSemaphoreTableName = _singleRowTableSemaphoreTableName;
        }

        #region ISemaphoreFactory Members

        public T GetSemaphore<T>(IDataHelper _dataHelper, string _semaphoreName) where T : ISemaphoreChecker
        {
            //NOTE:  HACK! :(
            if ("DateTime.Now".Equals(_semaphoreName)) return (T)(ISemaphoreChecker)new DateTimeNowSemaphoreChecker();
            if ("DateTime.UtcNow".Equals(_semaphoreName)) return (T)(ISemaphoreChecker)new DateTimeUTCNowSemaphoreChecker();

            if (m_rowPerSemaphoreNames.Contains(_semaphoreName))
            {
                ISemaphoreChecker retval = (ISemaphoreChecker)new RowPerSemaphoreCreator(_dataHelper, _semaphoreName);
                return (T)retval;
            }

            if (m_singleTableRowSemaphoreNames.Contains(_semaphoreName))
            {
                ISemaphoreChecker retval = (ISemaphoreChecker)new SingleTableRowSemaphoreSetter(m_singleRowTableSemaphoreConnectionString, m_singleRowTableSemaphoreTableName, _semaphoreName);
                return (T)retval;
            }

            throw new ArgumentOutOfRangeException(string.Format("Could not locate the semaphore called '{0}' in the factory's list of registered semaphores.  This could be a dynamic semaphore, or your configuration is out of date.", _semaphoreName));
        }

        #endregion
    }
}