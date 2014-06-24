using System;

namespace Tunney.Common.SanityChecks
{
    public interface ISanityChecker
    {
        /// <summary>
        /// Checks conditions BEFORE runtime gets going and the scheduler starts creating jobs.
        /// </summary>
        /// <exception cref="ApplicationException">Thrown when the sanity check fails for any reason.  Check derived exception type and message for more details.</exception>
        void Check();
    }
}