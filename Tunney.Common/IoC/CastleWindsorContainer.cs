using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

using Castle.Core;
using Castle.Core.Resource;

using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Releasers;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Installer;

using Quartz;

using Tunney.Common.IoC.CastleWindsor;
using Tunney.Common.Scheduling;

namespace Tunney.Common.IoC
{
    /// <summary>
    /// A very simple container wrapper for the Castle Windsor implementation of Inversion Of Control.    
    /// </summary>
    /// <remarks>Written by Stephen R. Tunney, Canada</remarks>
    [Serializable]
    public class CastleWindsorContainer : IIoCContainer, ISerializable, IDisposable , IInitializable
    {
        [NonSerialized]
        private readonly IWindsorContainer m_container;

        private readonly ILogger m_logger;
        private readonly string m_configFilename = null;
        private readonly IResource m_configResource = null;

        private CastleWindsorContainer(IWindsorInstaller _configInstaller, ILogger _logger)
        {
            if (null == _configInstaller) throw new ArgumentNullException(@"_configInstaller");
            if (null == _logger) throw new ArgumentNullException(@"_logger");            

            m_container = new WindsorContainer().Install(new CastleWindsor.TunneyCustomTypeConvertersInstaller()).Install(_configInstaller);
            m_container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
            //m_container = new WindsorContainer().Install(_configInstaller);
            m_logger = _logger;
        }

        public CastleWindsorContainer(ILogger _logger)
            : this(Configuration.FromAppConfig(), _logger)
        {            
        }

        public CastleWindsorContainer(string _configFilename, ILogger _logger)
            : this(Configuration.FromXmlFile(_configFilename), _logger)
        {            
            m_configFilename = _configFilename;
        }

        public CastleWindsorContainer(IResource _configResource, ILogger _logger)
            : this(Configuration.FromXml(_configResource), _logger)
        {
            m_configResource = _configResource;
        }

        #region IIoCContainer Members

        /// <summary>
        /// Gets the object that is configured in your IoC 
        /// configuration block with the given <paramref name="_id"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to return, can be generalized to something like
        /// 'object', or an interface or base class, or as specific as the
        /// actual concrete type of the object.
        /// </typeparam>
        /// <param name="_id">
        /// The string identifier used to find the configuration block with the desired
        /// object to retrieve and instantiate.
        /// </param>
        /// <returns>
        /// The configured object, or an exception with details.  You might want to put a breakpoint in the constructor
        /// of your target object if you run into problems.
        /// </returns>
        public virtual T Resolve<T>(string _id)
        {
            try
            {
                return m_container.Resolve<T>(_id);
            }
            catch (Exception _ex)
            {
                throw new Exception(string.Format("Could not resolve '{0}' from the container.", _id), _ex);
            }
        }

        #endregion

        #region ISerializable Members

        private const string SER_CONFIG_FILENAME = @"ConfigFilename";
        private const string SER_CONFIG_RESOURCE = @"ConfigResource";
        private const string SER_LOGGER          = @"Logger";

        protected CastleWindsorContainer(SerializationInfo info, StreamingContext context)
        {
            XmlInterpreter interpreter = null;
            if (!string.IsNullOrEmpty(info.GetString(SER_CONFIG_FILENAME)))
            {
                m_configFilename = info.GetString(SER_CONFIG_FILENAME);
                interpreter = new XmlInterpreter(m_configFilename);
            }
            else if(null != info.GetValue(SER_CONFIG_RESOURCE, typeof(IResource)))
            {
                m_configResource = (IResource)info.GetValue(SER_CONFIG_RESOURCE, typeof(IResource));
                interpreter = new XmlInterpreter(m_configResource);
            }

            if (null == interpreter)
            {
                interpreter = new XmlInterpreter();
            }

            m_logger = (ILogger)info.GetValue(SER_LOGGER, typeof(ILogger));
            
            m_container = new WindsorContainer(interpreter);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(SER_CONFIG_FILENAME, m_configFilename);
            info.AddValue(SER_CONFIG_RESOURCE, m_configResource);
            info.AddValue(SER_LOGGER, m_logger);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool _managedAndNativeClean)
        {
            if (_managedAndNativeClean)
            {
                //TODO:  Dispose unmanaged resources here if any exist.
            }

            m_container.Dispose();
        }

        #endregion

        #region IInitializable Members

        public virtual void Initialize()
        {
        }

        #endregion
    }
}