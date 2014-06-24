using System;

using Quartz;

using Tunney.Common.Scheduling.RunAs;

namespace Tunney.WorkExecutingConsole
{
    public static class Program
    {
        public const string EMERGENCY_LOGGER_NAME = @"Tunney.DefaultLogger";
        public const string MASTER_SCHEDULER_NAME = @"master_scheduler_starter";

        public static void Main(string[] args)
        {
            ConsoleProgram.ConsoleMain(EMERGENCY_LOGGER_NAME, MASTER_SCHEDULER_NAME);
        }
    }
}