using System;
using Tunney.Common.Data;

namespace Tunney.Common.Jobs
{
    public interface IDataHelperHolder
    {
        IDataHelper DataHelper { get; set; }
    }
}