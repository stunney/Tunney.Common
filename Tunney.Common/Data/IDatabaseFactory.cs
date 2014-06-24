using System;
using System.Data;
using System.Runtime.Serialization;

namespace Tunney.Common.Data
{
    public interface IDatabaseFactory : ISerializable
    {
        IDbConnection Create();

        IDataAdapter CreateDataAdapter(IDbCommand _command);

        void CreateTemporaryEntities(IDbConnection _createdConnection);

        void DropTemporaryEntities(IDbConnection _createdConnection);

        IDDLRunner DDLRunner { get; }
    }
}