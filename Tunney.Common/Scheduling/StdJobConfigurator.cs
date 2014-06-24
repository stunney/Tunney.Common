using System;
using System.Collections;
using System.Collections.Generic;
using Tunney.Common.IoC;
using System.Collections.Specialized;
using Tunney.Common.IoC.CastleWindsor;

namespace Tunney.Common.Scheduling
{
    [Serializable]
    public class StdJobConfigurator : IJobConfigurator
    {
        protected readonly IDictionary<string, string> m_keyListConfigurationIoCPairs;

        public StdJobConfigurator(IDictionary<KeyValuePair<string, string>, int> _blockingSemaphoreIoCNamesToCompareAndTimeGapInMinutes, IDictionary<string, string> _keyValueConfigurationPairs)
        {
            if (null == _blockingSemaphoreIoCNamesToCompareAndTimeGapInMinutes) throw new ArgumentNullException("_blockingSemaphoreIoCNamesToCompareAndTimeGapInMinutes");
            if (null == _keyValueConfigurationPairs) throw new ArgumentNullException("_keyValueConfigurationPairs");

            BlockingSemaphoreIoCNamesToCompareAndTimeGapInMinutes = _blockingSemaphoreIoCNamesToCompareAndTimeGapInMinutes;
            KeyValueConfigurationPairs = _keyValueConfigurationPairs;
            
        }

        public StdJobConfigurator(IDictionary<KeyValuePair<string, string>, int> _blockingSemaphoreIoCNamesToCompareAndTimeGapInMinutes, IDictionary<string, string> _keyValueConfigurationPairs, IDictionary<string, string> _keyListConfigurationPairs)
            : this(_blockingSemaphoreIoCNamesToCompareAndTimeGapInMinutes, _keyValueConfigurationPairs)
        {
            if (null == _keyListConfigurationPairs) throw new ArgumentNullException("_keyListConfigurationPairs");
            //KeyListConfigurationPairs = _keyListConfigurationPairs;
            m_keyListConfigurationIoCPairs = _keyListConfigurationPairs;
        }

        public virtual IDictionary<KeyValuePair<string, string>, int> BlockingSemaphoreIoCNamesToCompareAndTimeGapInMinutes { get; internal set; }
        public virtual IDictionary<string, string> KeyValueConfigurationPairs { get; internal set; }

        protected IDictionary<string, IList<string>> m_keyListConfigurationPairs;        

        public virtual IDictionary<string, IList<string>> KeyListConfigurationPairs// { get; internal set; }
        {            
            get
            {
                if (null == m_keyListConfigurationPairs)
                {
                    m_keyListConfigurationPairs = new Dictionary<string, IList<string>>(m_keyListConfigurationIoCPairs.Count);
                    foreach (string key in m_keyListConfigurationIoCPairs.Keys)
                    {
                        IList<string> entry = Container.Resolve<IList<string>>(m_keyListConfigurationIoCPairs[key]);
                        m_keyListConfigurationPairs.Add(key, entry);
                    }
                }
                return m_keyListConfigurationPairs;
            }
        }

        public virtual IIoCContainer Container { get; set; }
    }
}