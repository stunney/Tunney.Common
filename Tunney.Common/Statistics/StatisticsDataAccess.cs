using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using Tunney.Common.Statistics;

namespace Tunney.Common.Statistics
{
    [Serializable]
    public class StatisticsDataAccess : IStatisticsDataAccess, IDisposable
    {
        private static readonly object s_lockObject = new object();
        private static readonly object s_actionDurationAddLockObject = new object();

        private readonly string m_connectionString;

        private readonly SqlConnection m_connection;
        private readonly ActionDurationDataContext m_dataAccess;

        public StatisticsDataAccess(string _connectionString)
        {
            if (string.IsNullOrEmpty(_connectionString)) throw new ArgumentNullException(@"_connectionString");

            m_connectionString = _connectionString;

            m_connection = new SqlConnection(m_connectionString);

            m_connection.Open();

            //TODO:  TUNNEY!!!
            m_dataAccess = new ActionDurationDataContext(/*m_connection*/);
        }

        #region IStatisticsDataAccess Members

        public virtual void Add(int _actionID, Type _classType, string _source, TimeSpan _duration)
        {
            Add(_actionID, Environment.MachineName, string.Format("{0}::{1}", _classType.FullName, _source), _duration);
        }

        public virtual void Add(Action _action, string _machineName, string _source, TimeSpan _duration)
        {
            Add(_action, _machineName, _source, _duration);
        }

        public virtual void Add(int _actionID, string _machineName, string _source, TimeSpan _duration)
        {
            if (255 < _source.Length) _source = _source.Substring(0, 254); //Safegaurd for insert failures.

            ActionDuration ad = new ActionDuration();
            ad.ActionID = _actionID;
            ad.Duration = _duration.Ticks;
            ad.Machine = _machineName;
            ad.Source = _source;
            ad.Stamp = DateTime.Now;

            lock (s_actionDurationAddLockObject)
            {
                long newID = 1;
                if (m_dataAccess.ActionDurations.Count() > 0)
                {
                    newID = m_dataAccess.ActionDurations.Max(x => x.ID) + 1;
                }

                ad.ID = newID;

                m_dataAccess.ActionDurations.InsertOnSubmit(ad);
                m_dataAccess.SubmitChanges();
            }
        }

        public virtual IList<Action> GetActions()
        {
            lock (s_actionDurationAddLockObject)
            {
                return m_dataAccess.Actions.ToList<Action>().AsReadOnly();
            }
        }

        private static readonly object s_registerActionLockObject = new object();

        public virtual int RegisterAction(string _actionDescription)
        {
            int id = int.MinValue;
            lock (s_registerActionLockObject)
            {
                ActionDurationDataContext dataAccess = new ActionDurationDataContext(m_connection);

                id = FindActionID(_actionDescription);
                if (int.MinValue != id) return id;

                Action action = new Action();

                int newID = 1;
                if (dataAccess.Actions.Count() > 0)
                {
                    newID = dataAccess.Actions.Max(x => x.ID) + 1;
                }

                action.ID = newID;                     
                action.Action1 = _actionDescription;

                dataAccess.Actions.InsertOnSubmit(action);
                dataAccess.SubmitChanges();
            }
            id = FindActionID(_actionDescription);
            return id;
        }

        #endregion

        protected virtual int FindActionID(string _actionDescription)
        {
            foreach (Action a in GetActions())
            {
                if (a.Action1.Equals(_actionDescription))
                {
                    return a.ID;
                }
            }

            return int.MinValue;
        }

        #region ISerializable Members

        private const string SER_CONN_STR = @"ConnectionString";

        protected StatisticsDataAccess(SerializationInfo info, StreamingContext context)
        {
            if (null == info) throw new ArgumentNullException(@"info");

            m_connectionString = info.GetString(SER_CONN_STR);

            m_connection = new SqlConnection(m_connectionString);

            m_connection.Open();

            m_dataAccess = new ActionDurationDataContext(m_connection);
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (null == info) throw new ArgumentNullException(@"info");

            info.AddValue(SER_CONN_STR, m_connectionString);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (null != m_connection && ConnectionState.Open == m_connection.State) m_connection.Close();
        }

        #endregion
    }
}