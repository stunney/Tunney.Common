using System;
using System.Data;

namespace Tunney.Common.Data
{
    public interface IDDLGenerator
    {
        string RowIDColumnName { get; }
        string GenerateDDL(DataTable _schemaSource);
    }
}