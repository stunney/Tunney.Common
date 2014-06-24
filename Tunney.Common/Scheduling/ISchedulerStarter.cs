using System;
using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling
{
    public interface IScheduleStarter: IContainerUser, ILogWriter
    {
        void Start();
        void Stop();
        void Continue();
        void Pause();
    }
}