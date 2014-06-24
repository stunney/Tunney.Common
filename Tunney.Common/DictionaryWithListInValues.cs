using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tunney.Common
{
    public class DictionaryWithInt32KeyAndListValue<ListValueType>
    {
        IDictionary<int, IList<ListValueType>> m_values = new Dictionary<int, IList<ListValueType>>(10);

        public DictionaryWithInt32KeyAndListValue(IList<KeyValuePair<int, ListValueType>> _values)            
        {
            if (null == _values) throw new ArgumentNullException(@"_values");
            foreach (KeyValuePair<int, ListValueType> pair in _values)
            {
                if (!m_values.ContainsKey(pair.Key)) m_values.Add(pair.Key, new List<ListValueType>());
                m_values[pair.Key].Add(pair.Value);
            }
        }

        public virtual int Count { get { return m_values.Count; } }
    }
}
