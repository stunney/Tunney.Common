using System;
using System.ServiceProcess;

using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling.RunAs
{
    public static class ServiceProgram
    {
        public delegate ServiceBase CreateServiceBase(IScheduleStarter _masterSchedulerStarter);

        public static void ServiceMain(string _emergencyLoggerName, string _emergencyLoggerFilename, string _masterSchedulerStarterIoCId, CreateServiceBase _createCustomServiceCallback)
        {
            if (string.IsNullOrEmpty(_emergencyLoggerName)) throw new ArgumentNullException(@"_emergencyLoggerName");
            if (string.IsNullOrEmpty(_emergencyLoggerFilename)) throw new ArgumentNullException(@"_emergencyLoggerFilename");
            if (string.IsNullOrEmpty(_masterSchedulerStarterIoCId)) throw new ArgumentNullException(@"_masterSchedulerStarterIoCId");
            if (null == _createCustomServiceCallback) throw new ArgumentNullException(@"_createCustomServiceCallback");

            ILogger emergencyLogger = new Log4NetLogger(_emergencyLoggerFilename, _emergencyLoggerName);

            _ServiceMain(emergencyLogger, _masterSchedulerStarterIoCId, _createCustomServiceCallback);
        }

        public static void ServiceMain(string _emergencyLoggerName, string _masterSchedulerStarterIoCId, CreateServiceBase _createCustomServiceCallback)
        {
            if (string.IsNullOrEmpty(_emergencyLoggerName)) throw new ArgumentNullException(@"_emergencyLoggerName");            
            if (string.IsNullOrEmpty(_masterSchedulerStarterIoCId)) throw new ArgumentNullException(@"_masterSchedulerStarterIoCId");
            if (null == _createCustomServiceCallback) throw new ArgumentNullException(@"_createCustomServiceCallback");

            ILogger emergencyLogger = new Log4NetLogger(_emergencyLoggerName);

            _ServiceMain(emergencyLogger, _masterSchedulerStarterIoCId, _createCustomServiceCallback);
        }

        private static void _ServiceMain(ILogger _logger, string _masterSchedulerStarterIoCId, CreateServiceBase _createCustomServiceCallback)
        {
            IIoCContainer ioc = new CastleWindsorContainer(_logger);

            ServiceBase[] ServicesToRun = null;

            try
            {
                IScheduleStarter schedulerService = ioc.Resolve<IScheduleStarter>(_masterSchedulerStarterIoCId);

                ServicesToRun = new ServiceBase[1];
                ServicesToRun[0] = _createCustomServiceCallback(schedulerService);

                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                _logger.FATAL(ex);
                throw new ApplicationException("An unexpected error has occured.", ex);
            }
            finally
            {
                if (null != ServicesToRun)
                {
                    foreach (ServiceBase sb in ServicesToRun)
                    {
                        sb.Stop();
                    }
                }
            }
        }
    }
}