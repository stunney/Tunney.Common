using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

using Tunney;
using Tunney.Common.Data;
using Tunney.Common.IoC;
using Tunney.Common.Statistics;

namespace Tunney.Common.Data
{
    [Serializable]
    public class SQLServer_DataHelper : DataHelperConstants, IDataHelper, ISerializable
    {
        public event EventHandler<EventArgs<KeyValuePair<string, object>>> MetaDataAvailable;

        [NonSerialized]
        protected SerializedAccessSqlConnection m_cnConnection;
        [NonSerialized]
        protected SerializedAccessSqlConnection m_fnConnection;

        private static readonly object s_productionServerLockObject = new object();
        [NonSerialized]
        private readonly IDictionary<short, IDictionary<string, IList<string>>> m_productionServersByServerType = new Dictionary<short, IDictionary<string, IList<string>>>(10);

        protected readonly ILogger m_logger;
        protected readonly string m_cnConnectionString;                

        protected readonly IStatisticsDataAccess m_statisticsTracker;        

        protected static readonly string SERVERNAME;

        static SQLServer_DataHelper()
        {
            SERVERNAME = Environment.MachineName;
        }

        protected SQLServer_DataHelper()
        {            
        }

        public SQLServer_DataHelper(ILogger _logger)
            : this()
        {
            if (null == _logger)
            {
                throw new ArgumentNullException(@"_logger");
            }

            m_logger = _logger;
        }

        public SQLServer_DataHelper(string _cnConnectionString,                                        
                                        ILogger _logger,
                                        IStatisticsDataAccess _statisticsTracker)
            : this(_logger)
        {
            if (string.IsNullOrEmpty(_cnConnectionString)) throw new ArgumentNullException(@"_cnConnectionString");            
            
            if (null == _statisticsTracker) throw new ArgumentNullException(@"_statisticsTracker");
            
            m_cnConnectionString = _cnConnectionString;
            m_statisticsTracker = _statisticsTracker;

            try
            {
                m_cnConnection = new SerializedAccessSqlConnection(m_cnConnectionString);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid connection string", "_cnConnectionString", ex);
            }
        }

        #region ISerializable Members

        private const string SER_CN_CONN_STR = "CNCS";        
        private const string SER_LOGGER = @"Logger";

        protected SQLServer_DataHelper(SerializationInfo info, StreamingContext context)
            : this()
        {
            m_cnConnectionString = info.GetString(SER_CN_CONN_STR);            
            m_logger = (ILogger)info.GetValue(SER_LOGGER, typeof(ILogger));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(SER_CN_CONN_STR, m_cnConnectionString);            
            info.AddValue(SER_LOGGER, m_logger);
        }

        #endregion

        #region IDataHelper Members        

        /// <summary>
        /// Gets the Connection String used to connect to the requested data source.
        /// </summary>
        public virtual string ControllerNodeConnectionString
        {
            get { return m_cnConnectionString; }
        }

        /// <summary>
        /// Gets the Session count for each active SiteID
        /// from yesterday's TrafficMonitor data.
        /// </summary>
        /// <returns>
        /// A <see cref="IDictionary{Key,Value}"/> of SiteID(Key) to SessionCount(Value).
        /// </returns>
        public virtual IDictionary<long, long> GetSiteIDSessionCounts()
        {
            IDictionary<long, long> retval = new Dictionary<long, long>(1700);

            try
            {
                using(IDbConnection conn = ControllerNodeConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"[dbo].[Automation_GetYesterdaysSessionsPerSiteID]";
                        cmd.CommandType = CommandType.StoredProcedure;                        

                        cmd.Parameters.Add(CreateDebugParam(cmd));

                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            //SiteID, SessionCount, refer to stored proc PubSQL6.[TrafficMonitor].[dbo].[Automation_Get_SessionCount_Per_SiteID_From_Yesterday] for more details
                            retval.Add((long)dr[0], (long)dr[1]);
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(@"Unable to execute GetSiteIDSessionCounts", ex);
            }            

            return retval;
        }

        /// <summary>
        /// Gets the Smallest MaxDate
        /// for each of the following table combinations
        /// <list type="bullet">
        /// <item>Sessions + Events</item>
        /// <item>RevenueFactor</item>
        /// <item>Financials::tblOvertureData + Financials::DMRevenue</item>
        /// </list>
        /// </summary>
        /// <returns>
        /// A <see cref="IDictionary{Key, Value}"/> where the key is one of the items specifiec in the summary list, and the Value is a DateTime
        /// object representing the Smallest Max(DateTime) from the combination of the items specified for the Key.
        /// </returns>
        public virtual IDictionary<int, IDictionary<string, DateTime>> GetMinimumMaxDatesFromSourceAndTargetTables(string _sourceServerFQDN)
        {
            IDictionary<int, IDictionary<string, DateTime>> retval = new Dictionary<int, IDictionary<string, DateTime>>();

            //[Automation_Get_Minimum_MaxDateTime_From_All_Tables]

            try
            {
                using (IDbConnection conn = ControllerNodeConnection)
                {                
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"[Data].[dbo].[Automation_Get_Minimum_MaxDateTime_From_All_Tables]";
                        cmd.CommandType = CommandType.StoredProcedure;

                        //@FinancialsSourceServer
                        //@Debug

                        IDataParameter sourceServerParam = cmd.CreateParameter();
                        sourceServerParam.ParameterName = @"FinancialsSourceServer";
                        sourceServerParam.DbType = DbType.String;
                        sourceServerParam.Value = _sourceServerFQDN;

                        cmd.Parameters.Add(sourceServerParam);
                        cmd.Parameters.Add(CreateDebugParam(cmd));

                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                int SiteID = dr.GetInt32(3);
                                retval.Add(SiteID, new Dictionary<string, DateTime>());

                                //FinStamp
                                //SPSStamp
                                //RevFactorStamp
                                retval[SiteID].Add(@"FinStamp", dr.GetDateTime(0));
                                retval[SiteID].Add(@"SPSStamp", dr.GetDateTime(1));
                                retval[SiteID].Add(@"RevFactorStamp", dr.GetDateTime(2));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
                throw new ApplicationException(string.Format(@"Error executing GetMinimumMaxDatesFromSourceAndTargetTables({0})", _sourceServerFQDN), ex);
            }

            return retval;
        }

        public virtual IDictionary<int, DateTime> GetIncompleteSiteIDsFromRawFinancials(string _productionServerFQDN)
        {
            //Will return [ReportDate], [SiteID]
            //EXEC [Financials].[dbo].[Automation_Get_MinimumMaxDateTime_For_RevFactor] @Debug = ' + CONVERT(NVARCHAR(2), @Debug);

            IDictionary<int, DateTime> retval = new Dictionary<int, DateTime>();
            IDbConnection conn = ControllerNodeConnection;
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"[Financials].[dbo].[Automation_Get_MinimumMaxDateTime_For_RevFactor]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(CreateDebugParam(cmd));

                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DateTime date = dr.GetDateTime(0);
                            int siteID = dr.GetInt32(1);

                            if (retval.ContainsKey(siteID))
                            {
                                if (retval[siteID] < date) retval[siteID] = date;
                            }
                            else
                            {
                                retval.Add(siteID, date);
                            }
                        }
                    }
                }
            }
            return retval;
        }

        public virtual IDbConnection ControllerNodeConnection
        {
            get
            {
                if (null == m_cnConnection)
                {
                    if (string.IsNullOrEmpty(m_cnConnectionString))
                    {
                        throw new InvalidOperationException(@"Can not re-instantiate the Controller Node connection because the connection string is null or empty.");
                    }

                    m_cnConnection = new SerializedAccessSqlConnection(m_cnConnectionString);
                }

                return m_cnConnection.OpenedConnection;
            }
        }

        public virtual void OnMetaDataAvailable(string _name, object _data)
        {
            if (null != MetaDataAvailable)
            {
                OnMetaDataAvailable(new KeyValuePair<string, object>(_name, _data));
            }
        }

        public virtual void OnMetaDataAvailable(KeyValuePair<string, object> _value)
        {
            if (null != MetaDataAvailable)
            {
                MetaDataAvailable(this, new EventArgs<KeyValuePair<string, object>>(_value));
            }
        }

        public virtual IStatisticsDataAccess StatisticsTracker { get { return m_statisticsTracker; } }

        public virtual DateTime GetMaximumStampFromRawRevFactor(string _productionServerFQDN)
        {
            return DateTime.MinValue;
        }

        public virtual DateTime GetMaximumStampFromRawFinancials(string _productionServerFQDN)
        {
            return DateTime.MinValue;
        }

        /// <summary>
        /// Will ONLY query the ETL_Task_Time_Tracker_Semaphores table, as anything else will NOT adhere to this convention!
        /// </summary>
        /// <param name="_semaphoreNameFormat"></param>
        /// <returns></returns>
        public virtual IList<string> GetSemaphoreNameFormatMatches(string _semaphoreNameFormat)
        {
            List<string> retval = new List<string>(20);

            try
            {
                using(IDbConnection conn = ControllerNodeConnection)
                {
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format(@"SELECT [SemaphoreName] FROM [dbo].[ETL_Task_Time_Tracker_Semaphores] WITH (NOLOCK) WHERE [SemaphoreName] LIKE '%{0}%'", _semaphoreNameFormat);
                        cmd.CommandType = CommandType.Text;                        

                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                string val = (string)dr[0];
                                if (!string.IsNullOrEmpty(val)) retval.Add(val);
                            }
                        }                        
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(@"Unable to execute stored procedure called [Automation_Create_Time_Tracker_Sempahore].", ex);
            }            

            retval.TrimExcess();
            return retval;
        }

        #endregion

        protected virtual ILogger Logger
        {
            get { return m_logger; }
        }                

        public override string ToString()
        {
            string baseString = base.ToString();
            return string.Format(@"SQLServer_DataHelper:{0}", baseString.Substring(baseString.IndexOf(':') + 1));
        }
    }
}