using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Collections.Specialized;
using System.Web;
using System.Threading;

using Tunney;
using Tunney.Common.IoC;

namespace Tunney.Common.Data
{
    public class DataHelperConstants : IReferenceDataReader
    {
        public const string PROD_SERVER_TYPE_RAW = @"RAW";
        public const string PROD_SERVER_TYPE_FIN = @"FIN";
                
        public const string ETLDETAIL_PROD_SERVER = @"ProductionServer";
        public const string ETLDETAIL_PROD_SERVER_DB_CONN_STR_FORMAT = @"ProductionServerDbConnectionStringFormat";
        public const string ETLDETAIL_ETL_RUNNER_IOC_NAME = @"ETLRunnerIoCName";
        public const string ETLDETAIL_START_TIME_SEMAPHORE = @"SemaphoreCheckerObjectToGetStarTimeFrom";
        public const string ETLDETAIL_TIME_DELTA_IN_MINUTES = @"TimeDeltaPerIterationInMinutes";
        public const string ETLDETAIL_STARTTIME = @"ETLStartTime";
        public const string ETLDETAIL_ENDTIME = @"ETLEndTime";
        public const string ETLDETAIL_VERTICAL_PARTITION_ID = "VerticalPartitionID";

        public const string DATETIME_TOSTRING_FILENAMEFORMAT = @"yyyyMMddHHmmss";

        public static IDataParameter CreateDebugParam(IDbCommand _cmd)
        {
            // <pex>
            if (_cmd == (IDbCommand)null)
                throw new ArgumentNullException("_cmd");
            // </pex>
            IDbDataParameter debugParam = _cmd.CreateParameter();
            debugParam.ParameterName = @"@Debug";
            debugParam.DbType = DbType.Boolean;
            debugParam.Direction = ParameterDirection.Input;
            debugParam.Value = false;

            return debugParam;
        }

        public static IDataParameter CreateParameter(IDbCommand _cmd, string _name, DbType _type, IDataReader _currentRow)
        {
            // <pex>
            if (_currentRow == (IDataReader)null)
                throw new ArgumentNullException("_currentRow");
            // </pex>
            return CreateParameter(_cmd, _name, _type, _currentRow[_name]);
        }

        public static IDataParameter CreateParameter(IDbCommand _cmd, string _name, DbType _type, DataRow _currentRow)
        {
            // <pex>
            if (_currentRow == (DataRow)null) throw new ArgumentNullException("_currentRow");
            // </pex>

            object val = _currentRow[_name];
            if (val is DBNull) val = null;
            return CreateParameter(_cmd, _name, _type, val);
        }

        public static IDataParameter CreateParameter(IDbCommand _cmd, string _name, SqlDbType _type, DataRow _currentRow)
        {
            // <pex>
            if (_currentRow == (DataRow)null) throw new ArgumentNullException("_currentRow");
            // </pex>

            object val = _currentRow[_name];
            if (val is DBNull) val = null;
            return CreateParameter(_cmd, _name, _type, val);
        }

        public static IDataParameter CreateParameter(IDbCommand _cmd, string _name, SqlDbType _type, object _value)
        {
            // <pex>
            if (_cmd == (IDbCommand)null)
                throw new ArgumentNullException("_cmd");
            // </pex>
            SqlParameter retval = ((SqlCommand)_cmd).CreateParameter();
            retval.ParameterName = string.Format(@"@{0}", _name);
            retval.SqlDbType = _type;
            retval.Direction = ParameterDirection.Input;

            if (_value is DBNull || null == _value)
            {
                retval.Value = DBNull.Value;
            }
            else retval.Value = _value;

            return retval;
        }

        public static IDataParameter CreateParameter(IDbCommand _cmd, string _name, DbType _type, object _value)
        {
            // <pex>
            if (_cmd == (IDbCommand)null)
                throw new ArgumentNullException("_cmd");
            // </pex>
            IDbDataParameter retval = _cmd.CreateParameter();
            retval.ParameterName = string.Format(@"@{0}", _name);
            retval.DbType = _type;
            retval.Direction = ParameterDirection.Input;

            if (_value is DBNull || null == _value)
            {
                retval.Value = DBNull.Value;
            }
            else retval.Value = _value;

            return retval;
        }

        public static void TryToCloseConnection(IDbConnection _connection)
        {
            if (null == _connection) throw new ArgumentNullException(@"_connection");
                        
            //if (ConnectionState.Open == _connection.State)
            try
            {
                _connection.Close();
            }
            catch
            {
                //Gulp!!  YUMM!!!!  Intentional
            }
        }

        public static T CheckDictionaryForKeyAndValueType<T>(IDictionary<string, object> _toCheck, string _key)
        {
            if (!_toCheck.ContainsKey(_key) || null == _toCheck[_key]) throw new ArgumentException(string.Format(@"Expected arg '{0}' to exist and be a {1}", _key, typeof(T)), @"_toCheck");

            bool typeMatch = false;
            if (typeof(string) == typeof(T) &&
                _toCheck[_key] is string)
            {
                typeMatch = true;
                if (string.IsNullOrEmpty((string)_toCheck[_key]))
                {
                    return (T)(object)string.Empty; ///AHHHHHHAAAHAHHAHAHAHAHAHAHHAAAAAA!!!!!
                }
            }

            if (!typeMatch)
            {
                //T o = (T)System.Runtime.Serialization.FormatterServices.GetSafeUninitializedObject(typeof(T));

                typeMatch = typeof(T).IsInstanceOfType(_toCheck[_key]);

                if (!typeMatch)
                {
                    throw new InvalidCastException(string.Format("Can not cast an object of type {0} to {1}", _toCheck[_key].GetType().FullName, typeof(T).FullName));
                }
            }
            return (T)_toCheck[_key];
        }

        public virtual TimeZoneInfo GetDataCenterTimeZoneForProductionServer(string _productionServerFQDN, IDbConnection _openConnection)
        {
            //Someone was nice enough to write a view for me :)
            string SELECT = "SELECT [TimeZone] FROM [dbo].[ServerTimeZone] WHERE LOWER([ServerName]) = @ServerFQDN";

            //Get the timezone difference from the Reference::DataCenters->DataCenterTimeZone, and then update the SEStime column based on that where IPNumber IS NULL (Raw data, NOT historical data)

            //TODO:  This really should be an double, but the storage mechanism in the reference database only supports integers at the moment!  S.T.

            int timeZone = GetSingleValueFromSQL<int>(SELECT, _productionServerFQDN, _openConnection);

            if (-5 == timeZone)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneUtils.TIMEZONE_EST); //HACK!!! :)
            }
            else if (-8 == timeZone)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneUtils.TIMEZONE_PST);  //Just to ensure our current systems know what to expect!
            }
            else
            {
                foreach (TimeZoneInfo tz in TimeZoneInfo.GetSystemTimeZones())
                {
                    if (tz.BaseUtcOffset.Equals(TimeSpan.FromHours((double)timeZone))) //NOTE:  This could lead to using the wrong timezone.  We should possibly favour the north american timezones
                    {
                        return tz;
                    }
                }
            }
            
            throw new Exception(string.Format("Could not find the TimeZoneInfo related to the given production server '{0}'.", _productionServerFQDN));
        }

        public virtual int GetProductionServerID(string _productionServerFQDN, IDbConnection _openConnection)
        {
            const string SELECT = "SELECT [ServerID] FROM [Reference].[dbo].[ServerTimeZone] WHERE LOWER([ServerName]) = @ServerFQDN";

            //Get the timezone difference from the Reference::DataCenters->DataCenterTimeZone, and then update the SEStime column based on that where IPNumber IS NULL (Raw data, NOT historical data)

            return GetSingleValueFromSQL<int>(SELECT, _productionServerFQDN, _openConnection);
        }

        public static T GetSingleValueFromSQL<T>(string _sqlThatReturnsSingleIntValue, string _productionServerFQDN, IDbConnection _openConnection)
        {
            using (IDbCommand cmd = _openConnection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = _sqlThatReturnsSingleIntValue;

                _productionServerFQDN = _productionServerFQDN.ToLower();

                cmd.Parameters.Add(CreateParameter(cmd, "ServerFQDN", DbType.String, _productionServerFQDN));

                try
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read()) throw new ArgumentException(string.Format(@"Can not find the production server '{0}' int the [Reference]..[ServerTimeZone] view.", _productionServerFQDN));

                        T ret = (T)dr[0];
                        return ret;
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(string.Format("Failed to execute '{0}' against connection '{1}'.", _sqlThatReturnsSingleIntValue, _openConnection.ConnectionString), ex);
                }
            }
        }

        public virtual int ProcessCommandText_RECURSIVE(string _commandText, System.Data.IDbConnection _openConnection, int _recusiveCountDown)
        {
            using (System.Data.IDbCommand cmd = _openConnection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = _commandText;

                return ProcessCommand_RECURSIVE(cmd, 10);
            }
        }

        public virtual int ProcessCommand_RECURSIVE(System.Data.IDbCommand _command, int _recusiveCountDown)
        {
            if (null == _command) throw new ArgumentNullException("_command");

            if (string.IsNullOrEmpty(_command.CommandText)) throw new ArgumentException("CommandText can not be null or empty.", "_command");

            IDbTransaction trans = null;
            int retval = -1;
            try
            {
                if (null == _command.Transaction)
                {
                    trans = _command.Connection.BeginTransaction();
                    _command.Transaction = trans;
                }

                _command.CommandTimeout = 60000;
                retval = _command.ExecuteNonQuery();
                //TODO:  Log rows modified by a command!

                _command.Transaction.Commit();
            }
            catch (Exception ex)
            {
                if (null != trans) trans.Rollback();

                if (1 == _recusiveCountDown) throw;

                if (ex.Message.Contains("was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction."))
                {
                    Thread.Sleep(500); //Give the deadlock a chance to lift
                    _command.Transaction = null;
                    retval = ProcessCommand_RECURSIVE(_command, _recusiveCountDown - 1);
                }
                else throw new Exception(string.Format(@"Error executing SQLTransformation. SQL={0}", _command.CommandText), ex);
            }
            return retval;
        }        


        #region SqlBulkCopy Stuff
        
        public delegate void BulkCopyMappingCallback(SqlBulkCopy _bulkCopy, DataTable _sourceTable);

        public const SqlBulkCopyOptions SQLBULKCOPY_OPTIONS = SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock;

        public static void BulkCopyDataSetToTargetTable_RECURSIVE(DataTable _sourceTable, string _targetTableName, string _targetConnectionString, BulkCopyMappingCallback _mappingSetupCallback, int _retryAttemptsRemaining)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_targetConnectionString))
                {
                    connection.Open();
                    using (SqlTransaction trans = connection.BeginTransaction())
                    {
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SQLBULKCOPY_OPTIONS, trans))
                        {
                            bulkCopy.DestinationTableName = _targetTableName;
                            bulkCopy.BulkCopyTimeout = 60000;
                            bulkCopy.BatchSize = 25000;

                            _mappingSetupCallback(bulkCopy, _sourceTable);

                            try
                            {
                                bulkCopy.WriteToServer(_sourceTable);
                                trans.Commit();
                            }
                            finally
                            {
                                connection.Close();
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                if (0 == _retryAttemptsRemaining) throw;                

                Thread.Sleep(TimeSpan.FromSeconds(5.0d));

                BulkCopyDataSetToTargetTable_RECURSIVE(_sourceTable, _targetTableName, _targetConnectionString, _mappingSetupCallback, (_retryAttemptsRemaining - 1));
            }
        }

        #endregion

        public virtual string ParseDomain(string _host)
        {
            string[] parts = _host.Split('.');
            switch (parts.Length)
            {
                case 3:
                    if (parts[0].IndexOf("www") == 0) return parts[1];
                    else return parts[0];
                //break;
                case 2:
                    return parts[0];
                //break;
                case 1:
                case 0:
                    return string.Empty;
                //break;
                default: //More than 3 parts!
                    return parts[parts.Length - 3];
            };
            //return string.Empty;
        }

        public virtual string ParseRootDomain(string _host, string _domainString)
        {
            if (string.IsNullOrEmpty(_domainString)) return string.Empty;

            return _host.Substring(_host.LastIndexOf(_domainString), _host.Length - _host.LastIndexOf(_domainString));
        }

        public virtual string GetValueOrEmptyString(NameValueCollection _lookup, string _key)
        {
            if (new List<string>(_lookup.AllKeys).Contains(_key))
            {
                return _lookup[_key];
            }
            return string.Empty;
        }

        public virtual NameValueCollection ParseQueryString(string _queryString)
        {
            int httpIndex = _queryString.IndexOf("404;http");
            if (0 < httpIndex)
            {
                _queryString = _queryString.Substring(httpIndex);
            }

            string str = null;
            try
            {
                str = Uri.UnescapeDataString(_queryString);
            }
            catch
            {
                str = _queryString;
            }

            NameValueCollection preParsed = HttpUtility.ParseQueryString(str);

            NameValueCollection retval = new NameValueCollection();

            foreach (string key in preParsed.Keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    retval.Add("serveurl", preParsed[key]);
                }
                else
                {
                    string val = null;
                    try
                    {
                        val = Uri.UnescapeDataString(preParsed[key]);
                    }
                    catch
                    {
                        val = preParsed[key];
                    }

                    retval.Add(key, val.Trim());
                }
            }

            return retval;
        }
    }
}