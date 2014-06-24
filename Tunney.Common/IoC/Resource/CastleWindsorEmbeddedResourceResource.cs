using System;
using Castle.Core.Resource;
using System.IO;
using System.Reflection;

namespace Tunney.Common.IoC.Resource
{
    [Serializable]
    public class CastleWindsorEmbeddedResourceResource : IResource
    {
        private readonly Assembly m_assy;
        private readonly string m_resourceName;

        public CastleWindsorEmbeddedResourceResource(string _assemblyName, string _resourceName)
        {
            if (string.IsNullOrEmpty(_assemblyName)) throw new ArgumentNullException(@"_assemblyName");
            if (string.IsNullOrEmpty(_resourceName)) throw new ArgumentNullException(@"_resourceName");

            try
            {
                m_assy = Assembly.Load(_assemblyName);
                if (null == m_assy) throw new ArgumentException(@"_assemblyName");
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format(@"Could not load assembly named {0}", _assemblyName), ex);
            }

            m_resourceName = _resourceName;
        }

        #region IResource Members

        public virtual IResource CreateRelative(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) throw new ArgumentNullException(@"relativePath");
            return new StaticContentResource(GetLocalAssemblyEmbeddedResourceStringContents(relativePath));
        }

        private string GetLocalAssemblyEmbeddedResourceStringContents(string _resourceName)
        {
            if (string.IsNullOrEmpty(_resourceName)) throw new ArgumentNullException(@"_resourceName");

            using (StreamReader sr = new StreamReader(m_assy.GetManifestResourceStream(_resourceName)))
            {
                if (null == sr) throw new ArgumentException(string.Format("Embedded Resource Does Not Exist. '{0}'", _resourceName), @"_resourceName");

                return sr.ReadToEnd();
            }
        }

        public virtual string FileBasePath
        {
            get { return m_assy.FullName; }
        }

        public virtual System.IO.TextReader GetStreamReader(System.Text.Encoding encoding)
        {
            return new StreamReader(m_assy.GetManifestResourceStream(m_resourceName), encoding);
        }

        public virtual System.IO.TextReader GetStreamReader()
        {
            return new StreamReader(m_assy.GetManifestResourceStream(m_resourceName));
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {            
        }

        #endregion
    }
}