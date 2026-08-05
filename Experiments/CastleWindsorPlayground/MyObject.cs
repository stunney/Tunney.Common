using System;
using System.Collections.Generic;

namespace CastleWindsorPlayground
{
    public class MyObject
    {
        protected readonly IDictionary<int, string[]> m_stuff;

        public MyObject(IDictionary<int, string[]> _stuff)
        {
            if (null == _stuff) throw new ArgumentNullException(@"_stuff");

            m_stuff = _stuff;
        }

        public virtual int Count { get { return m_stuff.Count; } }
    }
}