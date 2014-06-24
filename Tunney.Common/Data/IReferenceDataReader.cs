using System;
using System.Data;

namespace Tunney.Common.Data
{
    public interface IReferenceDataReader
    {
        TimeZoneInfo GetDataCenterTimeZoneForProductionServer(string _productionServerFQDN, IDbConnection _openConnection);

        int GetProductionServerID(string _productionServerFQDN, IDbConnection _openConnection);
    }
}