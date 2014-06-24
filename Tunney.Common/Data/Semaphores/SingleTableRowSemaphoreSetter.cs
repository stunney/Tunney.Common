using System;
using System.Data;
using System.Data.SqlClient;

namespace Tunney.Common.Data.Semaphores
{
    [Serializable]
    public class SingleTableRowSemaphoreSetter : ASemaphoreSetter
    {
        //No need for [] here around our table name, we assume they are in the config!
        private const string SQL_UPDATE = @"UPDATE {0} SET [{1}] = @ColumnValue;";
        private const string SQL_SELECT = @"SELECT [{1}] FROM {0} WITH (NOLOCK);";

        protected readonly string m_dbConnectionString;
        protected readonly string m_tableName;

        protected readonly string m_select;
        protected readonly string m_update;

        public SingleTableRowSemaphoreSetter(string _dbConnectionString, string _tableName, string _semaphoreKey)
            : base(_semaphoreKey)
        {
            if (string.IsNullOrEmpty(_dbConnectionString)) throw new ArgumentNullException(@"_dbConnectionString");
            if (string.IsNullOrEmpty(_tableName)) throw new ArgumentNullException(@"_tableName");           

            m_dbConnectionString = _dbConnectionString;            
            m_tableName = _tableName;

            m_select = string.Format(SQL_SELECT, m_tableName, m_semaphoreKey);
            m_update = string.Format(SQL_UPDATE, m_tableName, m_semaphoreKey);
        }

        #region ISemaphoreSetter Members

        public override void Set(DateTimeOffset _value)
        {
            Write(_value);
        }

        #endregion

        #region ISemaphoreChecker Members

        public override DateTimeOffset Check()
        {
            return Read();
        }

        #endregion

        protected virtual void Write(DateTimeOffset _value)
        {
            using (IDbConnection conn = CreateConnection())
            {
                conn.Open();
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = m_update;

                    cmd.Parameters.Add(CreateParameter(cmd, "ColumnValue", DbType.DateTimeOffset, _value));
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        //TODO:  Possibly throw IFF rowsAffected != 1?
                    }
                    catch (SqlException sqlex)
                    {
                        throw new InvalidOperationException(string.Format("Error in sql statement '{0}'.  Attempting to set {1} to {2}", m_update, m_semaphoreKey, _value), sqlex);
                    }
                }
            }
        }

        protected virtual DateTimeOffset Read()
        {
            DateTimeOffset retval = DateTimeOffset.MinValue;
            using (IDbConnection conn = CreateConnection())
            {
                conn.Open();
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = m_select;

                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read() || 0 == dr.FieldCount)
                        {
                            throw new InvalidOperationException(string.Format("No semaphore key exists with the name {0} in the {1} located at {2}", m_semaphoreKey, m_tableName, m_dbConnectionString));
                        }

                        object o = dr[0];

                        if (null == o || o is DBNull)
                        {
                            retval = DateTime.MinValue;
                        }
                        else
                        {
                            retval = (DateTimeOffset)o;
                        }
                    }
                }
            }

            return retval;
        }

        protected virtual IDbConnection CreateConnection()
        {
            SqlConnection retval = new SqlConnection(m_dbConnectionString);
            return retval;
        }
    }
}