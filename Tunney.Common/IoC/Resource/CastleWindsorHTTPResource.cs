using System;
using Castle.Core.Resource;

namespace Tunney.Common.IoC.Resource
{
    [Serializable]
    public class CastleWindsorHTTPResource : IResource
    {
        public CastleWindsorHTTPResource(Uri _uri)
        {
            //TODO:  Finish!
            throw new NotImplementedException();
        }

        public CastleWindsorHTTPResource(string _uri)
        {
            //TODO:  Finish!
            throw new NotImplementedException();
        }

        #region IResource Members

        public IResource CreateRelative(string relativePath)
        {
            throw new NotImplementedException();
        }

        public string FileBasePath
        {
            get { throw new NotImplementedException(); }
        }

        public System.IO.TextReader GetStreamReader(System.Text.Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public System.IO.TextReader GetStreamReader()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}