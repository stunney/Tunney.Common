using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tunney.Common.IoC
{
    [Serializable]
    public class EventLogger : ILogger
    {
        public event EventHandler<EventArgs<string>> Info;
        public event EventHandler<EventArgs<string>> Debug;
        public event EventHandler<EventArgs<string>> Warn;
        public event EventHandler<EventArgs<string>> Error;
        public event EventHandler<EventArgs<string>> Fatal;

        #region ILogger Members

        public virtual void INFO(string _message)
        {
            if (null != Info) Info(this, new EventArgs<string>(_message));            
        }

        public virtual void INFO_Format(string _format, params object[] _formatParams)
        {
            if (null != Info) Info(this, new EventArgs<string>(string.Format(_format, _formatParams)));
        }

        public virtual void DEBUG(string _message)
        {
            if (null != Debug) Debug(this, new EventArgs<string>(_message));
        }

        public virtual void DEBUG_Format(string _format, params object[] _formatParams)
        {
            if (null != Debug) Debug(this, new EventArgs<string>(string.Format(_format, _formatParams)));
        }

        public virtual void WARN(string _message)
        {
            if (null != Warn) Warn(this, new EventArgs<string>(_message));
        }

        public virtual void WARN_Format(string _format, params object[] _formatParams)
        {
            if (null != Warn) Warn(this, new EventArgs<string>(string.Format(_format, _formatParams)));
        }

        public virtual void WARN(Exception _exception)
        {
            if (null != Warn) Warn(this, new EventArgs<string>(_exception.Message));
        }

        public virtual void ERROR(string _message)
        {
            if (null != Error) Error(this, new EventArgs<string>(_message));
        }

        public virtual void ERROR_Format(string _format, params object[] _formatParams)
        {
            if (null != Error) Error(this, new EventArgs<string>(string.Format(_format, _formatParams)));
        }

        public virtual void ERROR(Exception _exception)
        {
            if (null != Error) Error(this, new EventArgs<string>(_exception.Message));
        }

        public virtual void FATAL(Exception _exception)
        {
            if (null != Fatal) Fatal(this, new EventArgs<string>(_exception.Message));
        }

        #endregion

        public EventLogger()
        {
        }

        #region ISerializable Members

        protected EventLogger(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {            
        }

        #endregion
    }
}