using System;
using System.Collections.Generic;

namespace Tunney.Common.Data
{
    public interface IControllerNodeReader
    {
        /// <summary>
        /// Will ONLY query the ETL_Task_Time_Tracker_Semaphores table, as anything else will NOT adhere to this convention!
        /// </summary>
        /// <param name="_semaphoreNameFormat"></param>
        /// <returns></returns>
        IList<string> GetSemaphoreNameFormatMatches(string _semaphoreNameFormat);
    }
}