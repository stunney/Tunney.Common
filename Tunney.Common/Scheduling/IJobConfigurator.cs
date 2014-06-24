using System;
using System.Collections.Generic;
using Tunney.Common.IoC;

namespace Tunney.Common.Scheduling
{
    public interface IJobConfigurator : IContainerUser
    {
        IDictionary<KeyValuePair<string, string>, int> BlockingSemaphoreIoCNamesToCompareAndTimeGapInMinutes { get; }
        IDictionary<string, string> KeyValueConfigurationPairs { get; }

        IDictionary<string, IList<string>> KeyListConfigurationPairs { get; }
    }
}