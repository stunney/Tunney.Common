using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Tunney.Common.Data.Semaphores
{
    [Serializable]
    public class SemaphoreGroupComparer : SemaphoreComparer
    {
        public SemaphoreGroupComparer(ISemaphoreFactory _semaphoreFactory, IDataHelper _dataHelper)
            : base(_semaphoreFactory, _dataHelper)
        {
        }

        public override TimeSpan Diff(string _semaphoreNameAFormat, string _semaphoreNameB)
        {
            string semaphoreNameFormat = _semaphoreNameAFormat.Replace("{0}", string.Empty);

            IList<string> semaphoreNames = m_dataHelper.GetSemaphoreNameFormatMatches(semaphoreNameFormat);

            if (0 == semaphoreNames.Count) throw new ArgumentOutOfRangeException(string.Format(@"Could not find ANY semaphores that match the given format of '{0}'.", _semaphoreNameAFormat));

            ISemaphoreChecker semB = m_semaphoreFactory.GetSemaphore<ISemaphoreChecker>(m_dataHelper, _semaphoreNameB);

            TimeSpan maxGap = TimeSpan.MinValue;

            foreach(string semName in semaphoreNames)
            {
                ISemaphoreChecker semA = new RowPerSemaphoreSetter(m_dataHelper, semName); //Can't use semaphoreFactory, this is a dynamic semaphore!
                TimeSpan gap = (semA.Check() - semB.Check());
                if (gap > maxGap) maxGap = gap;
            }

            return maxGap;
        }
    }
}