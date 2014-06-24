using System;
using System.Collections.Generic;

namespace Tunney.Common.Data
{
    public interface IHistoricalDataReader
    {
        /// <summary>
        /// Gets the Session count for each active SiteID
        /// from yesterday's TrafficMonitor data.
        /// </summary>
        /// <returns>
        /// A <see cref="IDictionary{Key,Value}"/> of SiteID(Key) to SessionCount(Value).
        /// </returns>
        IDictionary<long, long> GetSiteIDSessionCounts();
    }
}