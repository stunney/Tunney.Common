using System;
using System.ServiceProcess;
using Tunney.Common.Scheduling;
using Tunney.Common.Scheduling.RunAs;

namespace Tunney.WorkExecutingService
{
	public static class Program
	{
		public const string EMERGENCY_LOGGER_NAME = @"Tunney.DefaultLogger";
		public const string MASTER_SCHEDULER_NAME = @"master_scheduler_starter";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		public static void Main()
		{
			ServiceProgram.ServiceMain(EMERGENCY_LOGGER_NAME, MASTER_SCHEDULER_NAME, CreateServiceBase);
		}

		private static ServiceBase CreateServiceBase(IScheduleStarter _masterSchedulerStarter)
		{
			Service1 retval = new Service1(_masterSchedulerStarter);
			return retval;
		}
	}
}