using System;
using System.Runtime.Serialization;
using Quartz;

namespace Tunney.Common.Scheduling
{
    public interface IJobScheduler : ISerializable
    {
        void Schedule(IScheduler _scheduler, JobDataMap _extendedDataMap);

        TimeSpan JobFrequency { get; set; }
    }
}