using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace Tunney.Common.Data
{
    [Serializable]
    public class SQLiteDataStore : IDataStore
    {
        public const string SQLITE_CONNECTION_STRING_FORMAT = @"Data Source={0};Version=3;New=true";
        public const string SQLITE_IN_MEMORY_CONNECTION_NAME = @":memory:";

        protected readonly string m_connFilename;

        protected readonly SQLiteConnection m_conn = new SQLiteConnection(string.Format(SQLITE_CONNECTION_STRING_FORMAT, SQLITE_IN_MEMORY_CONNECTION_NAME));

        public SQLiteDataStore()
        {
            OpenNewConnection();
        }

        public SQLiteDataStore(string _rootFolder)
        {
            if (string.IsNullOrEmpty(_rootFolder)) throw new ArgumentNullException(@"_rootFolder");

            m_connFilename = _rootFolder + Path.DirectorySeparatorChar + Path.GetRandomFileName() + @".db3";

            m_conn = new SQLiteConnection(string.Format(SQLITE_CONNECTION_STRING_FORMAT, m_connFilename));

            OpenNewConnection();
        }

        public SQLiteDataStore(SQLiteConnection _existingConnection)
        {
            if (null == _existingConnection) throw new ArgumentNullException(@"_existingConnection");

            m_conn = _existingConnection;

            OpenNewConnection();
        }

        ~SQLiteDataStore()
        {
            m_conn.Close();
            if (!string.IsNullOrEmpty(m_connFilename))
            {
                try
                {
                    //File.Delete(m_connFilename);
                }
                catch (IOException)
                {
                    //File likely being used somewhere else during debugging.  Ignore this for now.
                }
            }
        }

        public virtual void Vacuum()
        {
            const string vacuum = @"VACUUM;";
            if (!string.IsNullOrEmpty(m_connFilename))
            {
                using (IDbCommand cmd = m_conn.CreateCommand())
                {
                    cmd.CommandText = vacuum;
                    cmd.CommandType = CommandType.Text;
                    //cmd.ExecuteNonQuery();
                }
            }
            GC.Collect(2, GCCollectionMode.Forced);
        }

        protected virtual void OpenNewConnection()
        {
            if (ConnectionState.Open != m_conn.State) m_conn.Open();

            using (SQLiteCommand cmd = m_conn.CreateCommand())
            {
                cmd.CommandText = @"PRAGMA auto_vacuum = 1;";
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the existing, raw connection used in this instance.  Use with caution!!!!  Not thread-safe!!!!
        /// </summary>
        public virtual IDbConnection OpenConnection { get { return m_conn; } }

        public virtual IDataAdapter CreateDataAdapter(string _commandText)
        {
            return new SQLiteDataAdapter(_commandText, m_conn);
        }

        public virtual IDbCommand CreateCommand()
        {
            return m_conn.CreateCommand();
        }

        /// <summary>
        /// Adds data to the local datastore for you.
        /// </summary>
        /// <param name="_sourceData">
        /// The collection of data to inject
        /// </param>
        /// <remarks>
        /// <seealso cref="DateTimeOffset"/> is NOT supported!!!!
        /// </remarks>
        public virtual void StoreData(DataSet _sourceData)
        {
            IDDLGenerator gen = new SQLiteDDLGenerator();
            foreach (DataTable table in _sourceData.Tables)
            {
                string ddlSQL = gen.GenerateDDL(table);
                IDbCommand cmd = m_conn.CreateCommand();
                cmd.CommandText = ddlSQL;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }

            //DataSet ds2 = _sourceData.Copy(); //Why am I making a copy of the data?
            foreach (DataTable table2 in _sourceData.Tables)
            {
                foreach (DataRow dr in table2.Rows)
                {
                    dr.SetAdded();
                }
            }

            //NOTE:  Lots of this code came from here: http://sqlite.phxsoftware.com/forums/t/134.aspx

            foreach (DataTable table2 in _sourceData.Tables)
            {
                //using (IDbTransaction trans = m_conn.BeginTransaction())
                {
                    try
                    {
                        using (SQLiteCommand cmd = m_conn.CreateCommand())
                        {
                            StringBuilder insertCols = new StringBuilder(2000);
                            StringBuilder insertParams = new StringBuilder(2000);

                            Dictionary<string, IDbDataParameter> parameters = new Dictionary<string, IDbDataParameter>(100);

                            foreach (DataColumn dc in table2.Columns)
                            {
                                insertCols.AppendFormat(@"{0},", dc.ColumnName);

                                string paramName = string.Format(@"@{0}", dc.ColumnName);

                                insertParams.AppendFormat(@"{0},", paramName);

                                IDbDataParameter param = cmd.CreateParameter();
                                param.ParameterName = paramName;

                                parameters.Add(dc.ColumnName, param);
                                cmd.Parameters.Add(param);
                            }

                            insertCols.Remove(insertCols.Length - 1, 1);
                            insertParams.Remove(insertParams.Length - 1, 1);

                            cmd.CommandText = string.Format(@"INSERT INTO {0} ({1}) VALUES({2});", table2.TableName, insertCols, insertParams);

                            foreach (DataRow row in table2.Rows)
                            {
                                foreach (DataColumn dc in table2.Columns)
                                {
                                    parameters[dc.ColumnName].Value = row[dc.Ordinal];
                                }
                                cmd.ExecuteNonQuery();
                            }
                        }

                        //trans.Commit();
                    }
                    catch
                    {
                        //trans.Rollback();
                    }
                    finally
                    {
                        Vacuum();
                    }
                }
            }            
        }

        public virtual object ExecuteScalar(string _sql, bool _vacuumOnExit)
        {
            try
            {
                IDbCommand command = CreateCommand();
                command.CommandText = _sql;
                command.CommandType = CommandType.Text;
                object ret = command.ExecuteScalar();
                return ret;
            }
            finally
            {
                if(_vacuumOnExit) Vacuum();
            }
        }

        public virtual int ExecuteNonQuery(string _sql, bool _vacuumOnExit)
        {
            try
            {
                IDbCommand command = CreateCommand();
                command.CommandText = _sql;
                command.CommandType = CommandType.Text;
                int ret = command.ExecuteNonQuery();
                return ret;
            }
            finally
            {
                if (_vacuumOnExit) Vacuum();
            }
        }

        public virtual void DumpToFilesystem(string _filename)
        {
            Tunney.Common.Data.SQLite.SQLiteBackupProvider backupProvider = new Tunney.Common.Data.SQLite.SQLiteBackupProvider();

            //FileInfo tmpFile = new FileInfo(Path.GetTempFileName());
            backupProvider.BackupDatabaseToFile(_filename, m_conn);
            //return tmpFile.FullName;
        }
    }
}