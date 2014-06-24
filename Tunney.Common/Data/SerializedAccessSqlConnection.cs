using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Runtime.Serialization;

namespace Tunney.Common.Data
{
    [Serializable]
    public class SerializedAccessSqlConnection : ISerializable
    {
        [NonSerialized]
        private readonly Mutex m_connectionMutex = new Mutex();
        [NonSerialized]
        private IDbConnection m_connection;        

        private readonly string m_connectionString;

        public SerializedAccessSqlConnection(string _sqlConnectionString)
        {
            if (string.IsNullOrEmpty(_sqlConnectionString)) throw new ArgumentNullException(@"_sqlConnectionString");

            m_connectionString = _sqlConnectionString;
        }

        public virtual IDbConnection OpenedConnection
        {
            get
            {
                try
                {
                    IDbConnection retval = new SqlConnection(m_connectionString);
                    RepeatTryOpenConnection(retval);
                    return retval;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(string.Format("Failed connecting to '{0}'.", m_connectionString), ex);
                }
            }
        }

        protected virtual void RepeatTryOpenConnection(IDbConnection _conn, int _retryCount = 10)
        {
            while (true)
            {
                try
                {
                    _conn.Open();
                    break;
                }
                catch (Exception _ex)
                {
                    if (_ex.Message.Contains(@"Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding."))
                    {
                        _retryCount--;
                        if (0 == _retryCount) throw;
                        //else continue; //Not needed, here so you get the point :)
                    }
                }
            }
        }

        public virtual void Close()
        {
        }

        #region ISerializable Members

        private const string SER_CONN_STR = @"ConnectionString";

        protected SerializedAccessSqlConnection(SerializationInfo info, StreamingContext context)
        {
            m_connectionString = info.GetString(SER_CONN_STR);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(SER_CONN_STR, m_connectionString);
        }

        #endregion
    }
}