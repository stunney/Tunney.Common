using System;
using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling.RunAs
{
    public static class ConsoleProgram
    {
        public static void ConsoleMain(string _emergencyLoggerName, string _emergencyLoggerFilename, string _masterSchedulerStarterIoCId)
        {
            if (string.IsNullOrEmpty(_emergencyLoggerName)) throw new ArgumentNullException(@"_emergencyLoggerName");
            if (string.IsNullOrEmpty(_masterSchedulerStarterIoCId)) throw new ArgumentNullException(@"_masterSchedulerStarterIoCId");

            ILogger emergencyLogger = new Log4NetLogger(_emergencyLoggerFilename, _emergencyLoggerName);
            _ConsoleMain(emergencyLogger, _masterSchedulerStarterIoCId);
        }

        public static void ConsoleMain(string _emergencyLoggerName, string _masterSchedulerStarterIoCId)
        {
            if (string.IsNullOrEmpty(_emergencyLoggerName)) throw new ArgumentNullException(@"_emergencyLoggerName");            

            ILogger emergencyLogger = new Log4NetLogger(_emergencyLoggerName);
            _ConsoleMain(emergencyLogger, _masterSchedulerStarterIoCId);
        }

        private static void _ConsoleMain(ILogger _logger, string _masterSchedulerStarterIoCId)
        {
            IIoCContainer ioc = new CastleWindsorContainer(_logger);

            IScheduleStarter scheduleStarter = null;
            try
            {
                scheduleStarter = ioc.Resolve<IScheduleStarter>(_masterSchedulerStarterIoCId);                
                scheduleStarter.Start();

                Console.Write(@"Press enter to stop...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                _logger.FATAL(ex);
                throw;
            }
            finally
            {
                scheduleStarter.Stop();
            }
        }
    }
}