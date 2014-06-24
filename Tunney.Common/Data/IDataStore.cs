using System;
using System.Data;

namespace Tunney.Common.Data
{
    public interface IDataStore
    {
        IDbConnection OpenConnection { get; }
        IDataAdapter CreateDataAdapter(string _commandText);
        IDbCommand CreateCommand();

        void StoreData(DataSet _sourceData);

        object ExecuteScalar(string _sql, bool _vacuumOnExit);
        int ExecuteNonQuery(string _sql, bool _vacuumOnExit);

        void DumpToFilesystem(string _filename);
    }
}