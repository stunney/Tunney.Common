using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Tunney.Common.Statistics
{
    public interface IStatisticsDataAccess : ISerializable
    {
        void Add(Action _action, string _machineName, string _source, TimeSpan _duration);

        void Add(int _actionID, string _machineName, string _source, TimeSpan _duration);

        void Add(int _actionID, Type _classType, string _source, TimeSpan _duration);

        IList<Action> GetActions();

        int RegisterAction(string _actionDescription);
    }
}