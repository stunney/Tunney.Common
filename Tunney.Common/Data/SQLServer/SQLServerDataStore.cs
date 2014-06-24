using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Reflection;

namespace Tunney.Common.Data.SQLServer
{
    /// <summary>
    /// Remember that YOU are responsible for OPENING AND CLOSING the connection.  This class makes NO ASSUMPTIONS!
    /// </summary>
    public class SQLServerDataStore : IDataStore
    {
        protected readonly string m_connectionString;

        protected readonly SqlConnection m_connection;

        public SQLServerDataStore(string _connectionString)
        {
            if (string.IsNullOrEmpty(_connectionString)) throw new ArgumentNullException(@"_connectionString");
            m_connectionString = _connectionString;

            m_connection = new SqlConnection(m_connectionString);            
            m_connection.Open();                       
        }

        public virtual string DumpDatabaseName { get; set; }

        public virtual void StoreData(System.Data.DataSet _sourceData)
        {
            throw new NotImplementedException();
        }

        public virtual void DumpToFilesystem(string _filename)
        {
            ServerConnection connection = new ServerConnection(Connection);
            connection.Connect();
            Server sqlServer = new Server(connection);            
            Backup b = new Backup();
            b.Action = BackupActionType.Database;
            b.Database = DumpDatabaseName;
            b.Devices.AddDevice(_filename, DeviceType.File);
            b.SqlBackup(sqlServer);
            connection.Disconnect();
        }

        public virtual System.Data.IDbConnection OpenConnection
        {
            get
            {
                SqlConnection conn = Connection;
                conn.Open();
                return conn;
            }
        }

        public virtual System.Data.IDataAdapter CreateDataAdapter(string _commandText)
        {
            return new SqlDataAdapter(_commandText, Connection);
        }

        public virtual System.Data.IDbCommand CreateCommand()
        {
            return Connection.CreateCommand();
        }

        public virtual object ExecuteScalar(string _sql, bool _vacuumOnExit)
        {
            IDbConnection conn = Connection;
            conn.Open();
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = _sql;
            object retval = cmd.ExecuteScalar();
            conn.Close();
            return retval;
        }

        public virtual int ExecuteNonQuery(string _sql, bool _vacuumOnExit)
        {
            IDbConnection conn = Connection;
            conn.Open();
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = _sql;
            int retval = cmd.ExecuteNonQuery();
            conn.Close();
            return retval;
        }

        private SqlConnection Connection
        {
            get
            {
                return m_connection;
            }
        }
    }
}