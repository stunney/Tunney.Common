using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Tunney
{
    [Serializable]
    public class EventArgs<T> : EventArgs, ISerializable
    {
        private readonly T m_value;

        public EventArgs(T _value)
            : base()
        {
            m_value = _value;
        }

        public T Value
        {
            get { return m_value; }
        }

        #region ISerializable Members

        protected EventArgs(SerializationInfo info, StreamingContext context)
            : base()
        {
            m_value = (T)info.GetValue("Value", typeof(object));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Value", m_value);
        }

        #endregion
    }
}