using System;
using System.Collections.Generic;

namespace Tunney.Common.Data
{
    public interface IMetaDataNotifier
    {
        event EventHandler<EventArgs<KeyValuePair<string, object>>> MetaDataAvailable;

        void OnMetaDataAvailable(string _name, object _data);
        void OnMetaDataAvailable(KeyValuePair<string, object> _value);
    }
}
