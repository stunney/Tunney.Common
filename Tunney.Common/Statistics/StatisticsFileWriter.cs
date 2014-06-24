using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Tunney.Common.Statistics;
using Tunney.Common.IoC;

namespace Tunney.Common.Statistics
{
    [Serializable]
    public class StatisticsFileWriter : IStatisticsDataAccess, ILogWriter, IDisposable
    {
        protected readonly string m_baseFolderName;

        protected readonly FileInfo m_logFile;
        protected readonly StreamWriter m_logFileStreamWriter;

        protected readonly object m_registeredActionsLockObj = new object();
        protected int m_registeredActionLastInsertMaxValue = 0;
        protected readonly IDictionary<int, Action> m_registeredActions = new Dictionary<int, Action>(200);

        public StatisticsFileWriter(string _baseFolderName)
        {
            if (string.IsNullOrEmpty(_baseFolderName)) throw new ArgumentNullException(@"_baseFolderName");
            m_baseFolderName = _baseFolderName;

            DirectoryInfo di = new DirectoryInfo(_baseFolderName);

            if (!di.Exists) di.Create();

            di.Refresh();
            
            if(!di.Exists)
            {
                throw new DirectoryNotFoundException(string.Format(@"Could not find directory: {0}", _baseFolderName));
            }

            m_logFile = new FileInfo(string.Format(@"{0}{1}{2}_{3}_{4}_{5}.log", di.FullName, Path.DirectorySeparatorChar, DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss"), Environment.MachineName, @"StatsLog", Guid.NewGuid().ToString().Replace('-', '.')));

            m_logFileStreamWriter = m_logFile.CreateText();
        }

        #region IStatisticsDataAccess Members

        public virtual void Add(Action _action, string _machineName, string _source, TimeSpan _duration)
        {
            Add(_action, _machineName, _source, _duration);
        }

        public virtual void Add(int _actionID, string _machineName, string _source, TimeSpan _duration)
        {
            ActionDuration ad = new ActionDuration();
            ad.ActionID = _actionID;
            ad.Action = m_registeredActions[_actionID];
            ad.Duration = _duration.Ticks;
            ad.Machine = _machineName;
            ad.Source = _source;
            ad.Stamp = DateTime.Now;

            lock (m_registeredActionsLockObj)
            {
                m_registeredActions[_actionID].ActionDurations.Add(ad);

                m_logFileStreamWriter.WriteLine(string.Join(",", ad.Stamp, ad.Machine, ad.Action.Action1, ad.Source, ad.Duration));
                m_logFileStreamWriter.Flush();
            }
        }

        public virtual void Add(int _actionID, Type _classType, string _source, TimeSpan _duration)
        {
            Add(_actionID, Environment.MachineName, string.Format("{0}::{1}", _classType.FullName, _source), _duration);
        }

        public virtual IList<Action> GetActions()
        {
            lock (m_registeredActionsLockObj) return new List<Action>(m_registeredActions.Values).AsReadOnly();
        }

        public virtual int RegisterAction(string _actionDescription)
        {
            foreach (int key in m_registeredActions.Keys)
            {
                if(_actionDescription.ToLower().Equals(m_registeredActions[key].Action1)) return key;
            }

            lock(m_registeredActionsLockObj)
            {
                m_registeredActionLastInsertMaxValue++;
                
                Action a = new Action();
                a.Action1 = _actionDescription.ToLower();
                a.ID = m_registeredActionLastInsertMaxValue;

                m_registeredActions.Add(m_registeredActionLastInsertMaxValue, a);

                return m_registeredActionLastInsertMaxValue;
            }
        }

        #endregion

        #region ISerializable Members

        protected StatisticsFileWriter(SerializationInfo _info, StreamingContext _context)
        {
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {            
        }

        #endregion

        #region ILogWriter Members

        public virtual ILogger Logger { get; set; }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            if (null != m_logFileStreamWriter) m_logFileStreamWriter.Close();
        }

        #endregion
    }
}