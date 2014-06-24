using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;

namespace Tunney.Common.Data
{
    [Serializable]
    public class SQLServerDatabaseFactory : IDatabaseFactory
    {
        private const string CONNECTION_STRING = @"Data Source={0};Integrated Security=SSPI;";

        private readonly string m_machineName;
        private readonly IDDLRunner m_ddlRunner;
        private readonly string m_connectionStringFormat;

        public SQLServerDatabaseFactory(string _machineName)
        {
            if (string.IsNullOrEmpty(_machineName))
            {
                throw new ArgumentNullException(@"_machineName");
            }
            m_machineName = _machineName;
            m_connectionStringFormat = CONNECTION_STRING;
        }

        public SQLServerDatabaseFactory(string _machineName, string _alternativeConnectionStringFormat)
            : this(_machineName)
        {
            if (string.IsNullOrEmpty(_machineName))
            {
                throw new ArgumentNullException(@"_machineName");
            }

            if (string.IsNullOrEmpty(_alternativeConnectionStringFormat))
            {
                throw new ArgumentNullException(@"_alternativeConnectionStringFormat");
            }

            m_machineName = _machineName;
            m_connectionStringFormat = _alternativeConnectionStringFormat;
        }

        public SQLServerDatabaseFactory(string _machineName, IDDLRunner _ddlRunner)
            : this(_machineName)
        {
            if (null == _ddlRunner)
            {
                throw new ArgumentNullException(@"_ddlRunner");
            }
            
            m_ddlRunner = _ddlRunner;
        }        

        #region IDatabaseFactory Members

        public virtual IDDLRunner DDLRunner { get { return m_ddlRunner; } }

        public virtual System.Data.IDbConnection Create()
        {
            SqlConnection retval = new SqlConnection(string.Format(m_connectionStringFormat, m_machineName));
            retval.Open();
            return retval;
        }

        public virtual void CreateTemporaryEntities(IDbConnection _createdConnection)
        {
            if (null != m_ddlRunner)
            {
                m_ddlRunner.CreateEntities(_createdConnection);
            }
        }

        public virtual void DropTemporaryEntities(IDbConnection _createdConnection)
        {
            if (null != m_ddlRunner)
            {
                m_ddlRunner.DropEntities(_createdConnection);
            }
        }        

        public virtual System.Data.IDataAdapter CreateDataAdapter(System.Data.IDbCommand _command)
        {
            if (!(_command is SqlCommand))
            {
                throw new ArgumentException(@"Expected a SqlCommand", @"_command");
            }
            return new SqlDataAdapter((SqlCommand)_command);
        }

        #endregion

        #region ISerializable Members

        private const string SER_MACHINE_NAME = @"MachineName";
        private const string SER_DDL_RUNNER = @"DDLRunner";
        private const string SER_CONN_STR_FORMAT = "ConnectionStringFormat";

        public SQLServerDatabaseFactory(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            m_machineName = info.GetString(SER_MACHINE_NAME);
            m_connectionStringFormat = info.GetString(SER_CONN_STR_FORMAT);

            try
            {
                m_ddlRunner = (IDDLRunner)info.GetValue(SER_DDL_RUNNER, typeof(IDDLRunner));
            }
            catch
            {
            }
        }

        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue(SER_MACHINE_NAME, m_machineName);
            info.AddValue(SER_CONN_STR_FORMAT, m_connectionStringFormat);

            if (null != m_ddlRunner)
            {
                info.AddValue(SER_DDL_RUNNER, m_ddlRunner);
            }
        }

        #endregion
    }
}