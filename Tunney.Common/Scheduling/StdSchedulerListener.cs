using System;
using Quartz;
using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling
{
    public class StdSchedulerListener : ALoggingSchedulerListener, ISchedulerListener
    {
        public StdSchedulerListener(ILogger _logger)
            : base(_logger)
        {
        }
    }
}