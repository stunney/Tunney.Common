using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;

namespace Tunney.Common.Data
{
    [Serializable]
    public class DataSetDataStore : IDataStore
    {
        protected readonly DataSet m_dataSet = new DataSet();
        
        public DataSetDataStore()
        {
        }

        public virtual DataSet DataSet { get { return m_dataSet; } }

        public virtual IDbConnection OpenConnection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual IDataAdapter CreateDataAdapter(string _commandText)
        {
            throw new NotImplementedException();
        }

        public virtual IDbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }

        public virtual void StoreData(DataSet _sourceData)
        {
            m_dataSet.Merge(_sourceData);
        }

        public virtual int ExecuteNonQuery(string _sql, bool _vacuumOnExit)
        {
            throw new NotImplementedException();
        }

        public virtual object ExecuteScalar(string _sql, bool _vacuumOnExit)
        {
            throw new NotImplementedException();
        }

        public virtual void DumpToFilesystem(string _filename)
        {
            using (FileStream fs = new FileStream(_filename, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
            {
                using (DeflateStream zipper = new DeflateStream(fs, CompressionMode.Compress))
                {
                    Tunney.Serializer.Serialize(m_dataSet, zipper);
                    fs.Flush();
                }
            }
        }
    }
}