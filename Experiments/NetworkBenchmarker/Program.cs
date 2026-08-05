using System.ServiceProcess;

using Tunney.Common.Scheduling;
using Tunney.Common.Scheduling.RunAs;

namespace NetworkBenchmarker
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            Tunney.Common.Scheduling.ServiceProgram.ServiceMain(@"DefaultLogger", @"master_scheduler_starter", CreateService);
        }

        private static ServiceBase CreateService(IScheduleStarter _masterScheduleStarter)
        {
            return new Service1(_masterScheduleStarter);
        }
    }
}