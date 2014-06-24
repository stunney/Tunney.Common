using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

namespace Tunney.Common.Data
{
    public interface IDDLRunner : ISerializable
    {
        void CreateEntities(IDbConnection _openConnection);
        void DropEntities(IDbConnection _openConnection);

        IDictionary<string, string> TableNames {get;}
    }
}