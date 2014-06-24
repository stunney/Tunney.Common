using System;
using System.Runtime.Serialization;

namespace Tunney.Common.IoC
{
    public interface ILogger : ISerializable
    {
        void INFO(string _message);
        void INFO_Format(string _format, params object[] _formatParams);
        void DEBUG(string _message);
        void DEBUG_Format(string _format, params object[] _formatParams);
        void WARN(string _message);
        void WARN_Format(string _format, params object[] _formatParams);
        void WARN(Exception _exception);        
        void ERROR(string _message);
        void ERROR_Format(string _format, params object[] _formatParams);
        void ERROR(Exception _exception);
        void FATAL(Exception _exception);
    }
}