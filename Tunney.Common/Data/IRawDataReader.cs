using System;
using System.Collections.Generic;

namespace Tunney.Common.Data
{
    /// <summary>
    /// Signified a contract that garauntees that the implementer will provide data from ALL
    /// Raw (Sessions, Clicks, Financials, etc.) data sources.
    /// </summary>
    public interface IRawDataReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        DateTime GetMaximumStampFromRawRevFactor(string _productionServerFQDN);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_productionServerFQDN"></param>
        /// <returns></returns>
        DateTime GetMaximumStampFromRawFinancials(string _productionServerFQDN);

        /// <summary>
        /// Gets the Smallest MaxDate
        /// for each of the following table combinations
        /// <list type="bullet">
        /// <item>Sessions + Events</item>
        /// <item>RevenueFactor</item>
        /// <item>Financials::tblOvertureData + Financials::DMRevenue</item>
        /// </list>
        /// </summary>
        /// <returns>
        /// A <see cref="IDictionary{Key, Value}"/> where the key is one of the items specifiec in the summary list, and the Value is a DateTime
        /// object representing the Smallest Max(DateTime) from the combination of the items specified for the Key.
        /// </returns>
        IDictionary<int, IDictionary<string, DateTime>> GetMinimumMaxDatesFromSourceAndTargetTables(string _sourceServerFQDN);
    }
}