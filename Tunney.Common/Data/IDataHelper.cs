using System;
using System.Data;
using Tunney.Common.Data;
using Tunney.Common.Statistics;

namespace Tunney.Common.Data
{
    public interface IDataHelper : IControllerNodeReader, IRawDataReader, IHistoricalDataReader, IMetaDataNotifier, IReferenceDataReader
    {
        /// <summary>
        /// Gets the Connection String used to connect to the requested controller node data source.
        /// </summary>
        string ControllerNodeConnectionString { get; }

        IDbConnection ControllerNodeConnection { get; }        

        IStatisticsDataAccess StatisticsTracker { get; }
    }
}