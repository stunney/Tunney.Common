using System;
using Tunney.Common.Data;

namespace Tunney.Common.SanityChecks
{
    [Serializable]
    public class SQLiteSanityChecker : ISanityChecker
    {
        public SQLiteSanityChecker()
        {
        }

        public void Check()
        {
            try
            {
                SQLiteDataStore ds = new SQLiteDataStore();
                int ret = ds.ExecuteNonQuery(@"CREATE TABLE test123 (col1 INTEGER NOT NULL);", false);
            }
            catch (Exception _ex)
            {
                throw new ApplicationException(_ex.Message, _ex);
            }
        }
    }
}
