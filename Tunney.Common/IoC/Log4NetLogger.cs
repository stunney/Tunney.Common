using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

using log4net;
using log4net.Config;

namespace Tunney.Common.IoC
{
    [Serializable]
    public class Log4NetLogger : ILogger
    {
        [NonSerialized]
        private readonly ILog m_logger;

        private readonly string m_loggerName;
        private readonly string m_loggerFile;

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public Log4NetLogger()
        {
            XmlConfigurator.Configure();
            //BasicConfigurator.Configure(); //Does NOT work properly!

            m_logger = LogManager.GetLogger(string.Empty);
            SetGlobalContext();
        }

        /// <summary>
        /// Uses App.config values to configure the logger
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public Log4NetLogger(string _log4netLoggerName)            
        {
            if (string.IsNullOrEmpty(_log4netLoggerName)) throw new ArgumentNullException(@"_log4netLoggerName");

            XmlConfigurator.Configure();
            //BasicConfigurator.Configure(); //Does NOT work properly!

            m_loggerName = _log4netLoggerName;

            //m_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            m_logger = LogManager.GetLogger(m_loggerName);

            SetGlobalContext();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public Log4NetLogger(string _filename, string _log4netLoggerName)
        {
            if (string.IsNullOrEmpty(_filename)) throw new ArgumentNullException(@"_filename");
            if (string.IsNullOrEmpty(_log4netLoggerName)) throw new ArgumentNullException(@"_log4netLoggerName");

            FileInfo configFile = new FileInfo(_filename);

            if (!configFile.Exists)
            {
                if (_filename.StartsWith("~") || _filename.StartsWith("."))
                {
                    _filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + _filename.Substring(1); //Remove ('~' or '.')
                }
                else
                {
                    throw new FileNotFoundException(@"Could not find log4net config file", _filename);
                }                
            }

            try
            {
                XmlConfigurator.Configure(configFile);
            }
            catch
            {
                throw;
            }

            m_loggerName = _log4netLoggerName;
            m_loggerFile = _filename;

            m_logger = LogManager.GetLogger(GetType());
            SetGlobalContext();
        }

        #region ILogger Members

        public virtual void INFO(string _message)
        {
            if(m_logger.IsInfoEnabled)
                m_logger.Info(_message);
        }

        public virtual void INFO_Format(string _format, params object[] _formatParams)
        {
            if (m_logger.IsInfoEnabled)
                m_logger.Info(string.Format(_format, _formatParams));
        }

        public virtual void DEBUG(string _message)
        {
            if (m_logger.IsDebugEnabled)
                m_logger.Debug(_message);
        }

        public virtual void DEBUG_Format(string _format, params object[] _formatParams)
        {
            if (m_logger.IsDebugEnabled)
                m_logger.Debug(string.Format(_format, _formatParams));
        }

        public virtual void WARN(string _message)
        {
            if(m_logger.IsWarnEnabled)
                m_logger.Warn(_message);
        }

        public virtual void WARN_Format(string _format, params object[] _formatParams)
        {
            if (m_logger.IsWarnEnabled)
                m_logger.Warn(string.Format(_format, _formatParams));
        }

        public virtual void WARN(Exception _exception)
        {
            if (m_logger.IsWarnEnabled)
                m_logger.Warn(_exception.Message, _exception);
        }

        public virtual void ERROR(string _message)
        {
            if(m_logger.IsErrorEnabled)
                m_logger.Error(_message);
        }

        public virtual void ERROR_Format(string _format, params object[] _formatParams)
        {
            if (m_logger.IsErrorEnabled)
                m_logger.Error(string.Format(_format, _formatParams));
        }

        public virtual void ERROR(Exception _exception)
        {
            if (m_logger.IsErrorEnabled)
                m_logger.Error(_exception.Message, _exception);
        }

        public virtual void FATAL(Exception _exception)
        {
            if(m_logger.IsFatalEnabled)
                m_logger.Fatal(_exception.Message, _exception);
        }

        #endregion

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected virtual void SetGlobalContext()
        {            
            GlobalContext.Properties["MachineName"] = Environment.MachineName;
            GlobalContext.Properties["ProcessName"] = Process.GetCurrentProcess().ProcessName;
            GlobalContext.Properties["ProcessID"] = Process.GetCurrentProcess().Id.ToString();
            GlobalContext.Properties["ProcessArguments"] = string.Join(" ", Environment.GetCommandLineArgs(), 1, Environment.GetCommandLineArgs().Length - 1);
            GlobalContext.Properties["CurrentDirectory"] = Environment.CurrentDirectory;
        }

        #region ISerializable Members

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected Log4NetLogger(SerializationInfo info, StreamingContext context)
        {
            m_loggerName = info.GetString("Log4NetLoggerName");
            try
            {
                m_loggerFile = info.GetString("Filename");
            }
            catch (Exception)
            {
                m_loggerFile = null;
            }

            if (string.IsNullOrEmpty(m_loggerFile))
            {
                XmlConfigurator.Configure();                
            }
            else
            {
                XmlConfigurator.Configure(new FileInfo(m_loggerFile));
            }

            m_logger = LogManager.GetLogger(m_loggerName);
            SetGlobalContext();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Log4NetLoggerName", m_loggerName);
            info.AddValue("Filename", (null == m_loggerFile) ? string.Empty : m_loggerFile);
        }

        #endregion
    }
}