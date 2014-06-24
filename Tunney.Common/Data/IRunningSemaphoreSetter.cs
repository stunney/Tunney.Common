using System;
using System.Collections.Generic;

namespace Tunney.Common.Data
{
    public interface IRunningSemaphoreSetter
    {
        void Set(bool _isRunning);
    }
}
