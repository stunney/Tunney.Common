using System;
using System.Collections.Generic;

namespace Tunney.Common.Statistics
{
    [Serializable]
    public class NullStatisticsWriter : IStatisticsDataAccess, IDisposable
    {
        public NullStatisticsWriter()
        {
        }

        protected NullStatisticsWriter(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
        }

        public virtual void Add(Action _action, string _machineName, string _source, TimeSpan _duration)
        {            
        }

        public virtual void Add(int _actionID, string _machineName, string _source, TimeSpan _duration)
        {            
        }

        public virtual void Add(int _actionID, Type _classType, string _source, TimeSpan _duration)
        {            
        }

        public virtual IList<Action> GetActions()
        {
            return new List<Action>(0).AsReadOnly();
        }

        public virtual int RegisterAction(string _actionDescription)
        {
            return 0;
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {            
        }

        public virtual void Dispose()
        {
        }
    }
}