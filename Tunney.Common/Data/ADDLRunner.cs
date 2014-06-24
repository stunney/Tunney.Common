using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

using Tunney.Common.IoC;

namespace Tunney.Common.Data
{
    [Serializable]
    public abstract class ADDLRunner : IDDLRunner
    {
        public abstract void CreateEntities(IDbConnection _openConnection);

        public virtual void DropEntities(IDbConnection _openConnection)
        {
            foreach (string tableName in m_tableNames.Values)
            {
                try
                {
                    using (IDbCommand cmd = _openConnection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 60000;
                        cmd.CommandText = string.Format(@"DROP TABLE [{0}];", tableName);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains(@"because it does not exist"))
                    {
                        throw;
                    }
                }
            }
        }

        protected readonly IDictionary<string, string> m_tableNames = new Dictionary<string, string>();

        private ADDLRunner()
        {
        }

        protected ADDLRunner(IDictionary<string, string> _tableNames)
        {
            foreach (string key in _tableNames.Keys)
            {
                m_tableNames.Add(key, _tableNames[key]);
            }
        }

        public virtual IDictionary<string, string> TableNames { get { return m_tableNames; } }

        protected virtual void RunDDL(string _ddlSql, IDbConnection _openConnection)
        {
            if (null == _openConnection) throw new ArgumentNullException(@"_openConnection");
            if (string.IsNullOrEmpty(_ddlSql)) throw new ArgumentNullException(@"_ddlSql");

            try
            {
                using (IDbTransaction trans = _openConnection.BeginTransaction())
                {
                    using (IDbCommand cmd = _openConnection.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = _ddlSql;
                        cmd.CommandTimeout = 120000;

                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
            }
            catch (DbException _dbex)
            {
                if (!_dbex.Message.StartsWith(@"There is already an object named '"))
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(@"Unable to create a table with sql={0}", _ddlSql), ex);
            }
        }

        #region ISerializable Members

        private const string SER_TABLE_NAMES = @"TableNames";

        protected ADDLRunner(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (null == info) throw new ArgumentNullException(@"info");

            m_tableNames = (IDictionary<string, string>)info.GetValue(SER_TABLE_NAMES, typeof(IDictionary<string, string>));

            if (null == m_tableNames) throw new NullReferenceException("Must have a tables list to account for.");
            if (0 == m_tableNames.Count) throw new InvalidProgramException("Must have SOME tables to account for.");
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue(SER_TABLE_NAMES, m_tableNames);
        }

        #endregion
    }
}